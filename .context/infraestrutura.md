# Infraestrutura e entrega (`infra/`, `.github/`, Dockerfiles)

Ambiente de **apresentação**: um único nó EC2 com **k3s**, dimensionado para custo baixo. Passo a passo operacional em
[`infra/README.md`](../infra/README.md); aqui ficam a arquitetura e as regras para quem altera a infraestrutura.

## Visão geral

```
GitHub (push main) ──CI──▶ imagens + chart ──▶ GHCR (ghcr.io/<owner>/<repo>/…) ◀──┐
                                                                                   │ lê chart novo (OCI)
Repositório: infra/gitops/<ambiente> (HelmRelease) ◀── lê a cada 1 min ── Flux (no k3s) ──helm upgrade──▶ app
                                                                                   │
Internet ──80/443──▶ EC2 (EIP) ─▶ ServiceLB do k3s ─▶ ingress-nginx ─▶ /api → BFF · / → web ◀──────┘
                                                                        BFF → core-service (gRPC) → PostgreSQL
                                                                        BFF → Redis · Elasticsearch
                                                                        BFF → storage-service (gRPC) → S3 (AWS) · MongoDB · RabbitMQ
Seu kubectl ──6443 (admin_cidrs) ou túnel SSM──▶ API do k3s (contexto fix-<ambiente>)
```

| Peça | Onde | Papel |
| --- | --- | --- |
| Dockerfiles | `backend/core-service/Dockerfile`, `frontend/bff/Dockerfile`, `frontend/web/Dockerfile` | Build a partir da **raiz** do repositório (`.dockerignore` na raiz); core e BFF precisam de `protos/` |
| Chart Helm | `infra/helm/fix` | Plataforma inteira: 3 apps + PostgreSQL, Redis, Elasticsearch + Ingress |
| Terraform | `infra/terraform` | VPC mínima, EC2 + EIP, SG, IAM do nó, segredos no SSM, bootstrap (user-data) |
| GitOps | `infra/gitops/<ambiente>` | O que o cluster aplica: HelmRelease (versão do chart + values do ambiente); `infra/gitops/flux` liga o Flux ao repositório |
| Scripts | `infra/scripts/kubeconfig.{ps1,sh}` | Colocam o cluster no `kubectl` local |
| Pipelines | `.github/workflows/ci.yml`, `deploy.yml` | CI em PR; na `main`: CI → imagens → chart (o Flux aplica; o GitHub não acessa a AWS nem o cluster) |

## Imagens

| Imagem | Base | Porta | Observações |
| --- | --- | --- | --- |
| `core-service` | `dotnet/sdk:10.0` → `dotnet/aspnet:10.0` | 5098 (h2c) | Usuário não-root `$APP_UID`; `ASPNETCORE_ENVIRONMENT=Production` |
| `storage-service` | `dotnet/sdk:10.0` → `dotnet/aspnet:10.0` | 5099 (h2c) | Uploads; compila `protos/` na Infrastructure |
| `bff` | `node:22-alpine` | 3000 | `npm prune --omit=dev`; protos em `/app/protos`; roda com `--import ./dist/instrumentation.js` |
| `web` | `node:22-alpine` → `nginx-unprivileged` | 8080 | SPA com fallback para `index.html`, `/healthz`; chama a API em `/api` (mesmo origin) |

## Chart `fix`

- Nomes: `<release>-<componente>` (`fix-core-service`, `fix-storage-service`, `fix-bff`, `fix-web`, `fix-postgres`, `fix-redis`,
  `fix-elasticsearch`, `fix-mongodb`, `fix-rabbitmq`; `fix-s3` só com `objectStorage.provider: internal`).
- **Uploads**: arquivos no bucket S3 do Terraform (`s3.tf`: privado, AES256, só HTTPS, usuário IAM restrito ao bucket —
  os pods não alcançam as credenciais do nó); `s3-bucket`, `s3-region`, `s3-access-key`, `s3-secret-key` chegam ao
  `fix-secrets` pelo `fix-sync`. Senhas internas do Mongo/RabbitMQ: Secret `fix-storage-secrets`, gerado pelo chart no
  primeiro deploy e preservado (`lookup` + `resource-policy: keep`). Ingress aceita corpo de até 25 MB (uploads de 20 MB).
- Imagem: `<image.registry>/<componente>:<tag>`, com `tag` = `image.tag` ou, por padrão, a **appVersion** do chart. O pipeline
  publica o chart com `appVersion=<sha>`: cada versão do chart instala as imagens geradas no mesmo build (Packages do GitHub).
- Segredos vêm do Secret `fix-secrets` (`postgres-password`, `session-secret`, `admin-email`, `admin-password`,
  `grafana-admin-password`), criado no nó pelo `fix-sync` a partir do SSM. `secrets.create=true` só para testes locais.
- Valores do ambiente (domínios, TLS, observabilidade, registry) ficam **no repositório**, em
  `infra/gitops/<ambiente>/fix.yaml` (HelmRelease). Push na `main` = o Flux aplica.
- Ingress `nginx`: `/api` → BFF, `/` → web. O core-service **não** é exposto. Com `ingress.tls.enabled`, o cert-manager emite
  o certificado (ClusterIssuer `letsencrypt`).
- core-service roda as migrations no startup (`Database__MigrateOnStartup=true`) e cria o super administrador (`Seed__*`);
  `startupProbe` dá até 3 min para isso. Init containers aguardam PostgreSQL (core) e core + Elasticsearch (BFF).
- **Observabilidade no cluster** (`observability.enabled`, ligada no ambiente de apresentação): OTel Collector, Prometheus,
  Jaeger e Grafana, equivalentes a `observability/`; a telemetria das apps liga sozinha e aponta para o coletor.
  Grafana em `observability.grafana.host` (admin + senha do SSM), com os datasources no template do chart e os dashboards
  copiados de `observability/grafana/dashboards` pelo pipeline ao empacotar (fonte única: editar lá reflete no cluster no
  próximo deploy). A UI do Jaeger fica em `<grafana host>/jaeger`, liberada pelo ingress-nginx só com sessão válida no Grafana
  (`auth-url` → `/api/user`). Com tudo ligado o cluster usa ~2,9 GB.
- Segurança dos pods: `runAsNonRoot`, sem escalonamento de privilégio, capabilities removidas; init containers como 65534.
- Recursos pequenos por padrão (ver `values.yaml`); Elasticsearch com heap de 384 MB.

## Nó (bootstrap em `templates/user-data.sh.tftpl`)

Amazon Linux 2023 (SSM Agent e AWS CLI nativos) → swap 2 GB e `vm.max_map_count` → k3s (`--disable traefik`,
`--tls-san <EIP>`) → Helm → ingress-nginx (Service LoadBalancer publicado nas portas 80/443 pelo ServiceLB do k3s) →
cert-manager → `/usr/local/bin/fix-sync` (Secret `fix-secrets` e ClusterIssuer `letsencrypt` a partir do SSM) →
Flux (chart `fluxcd-community/flux2`, só source/kustomize/helm controllers, ~75 MB) + GitRepository/Kustomization apontando
para `infra/gitops/<ambiente>` →
kubeconfig publicado no SSM (`/fix/<ambiente>/kubeconfig`, contexto `fix-<ambiente>`, server = EIP) → marca
`/var/lib/fix/bootstrap-done`. Log em `/var/log/fix-bootstrap.log`.

Versões fixadas em variáveis: k3s `v1.33.5+k3s1`, Helm `v3.19.1`, ingress-nginx `4.15.1`, cert-manager `v1.21.2`,
Flux chart `2.19.1` (Flux v2.9.5). O Terraform ignora mudanças de `user_data`/AMI na instância existente: mudanças no
bootstrap só valem com `terraform apply -replace=aws_instance.node` (recria o cluster e perde os dados).

## Deploy (GitOps com Flux)

- O pipeline publica o chart `0.1.<run_number>` com `appVersion=<sha>` depois das imagens do mesmo build.
- `infra/gitops/<ambiente>/fix.yaml`: `HelmRepository` (OCI no GHCR) + `HelmRelease` `fix` com `version: ">=0.1.0"` (sempre o
  chart mais novo; troque por versão exata para fixar/voltar) e os values do ambiente. O Flux verifica a cada 1–2 min.
- Upgrade com remediação: duas falhas seguidas voltam para a versão anterior.
- O Flux assume o release `fix` existente sem reinstalar (dados preservados) — testado.
- `fix-sync` (no nó) é a única ponte com a AWS: Secret `fix-secrets`, ClusterIssuer e, opcional, credencial do GHCR.
  O script é gravado pelo user-data (ignorado depois da criação): mudou o template? Atualize o nó existente com
  `aws ssm send-command` (o mesmo conteúdo renderizado em `/usr/local/bin/fix-sync`) e rode-o.

## Regras

- Segredo nunca vai para o repositório, o chart ou o workflow: fica no SSM (e no estado do Terraform — proteja o estado).
- O GitHub não tem acesso à AWS nem ao cluster: o cluster puxa do repositório e do GHCR (modelo pull do GitOps).
- Configuração do ambiente é código: altere `infra/gitops/<ambiente>` por commit, não com `helm upgrade`/`kubectl edit` no
  cluster (o Flux desfaz mudanças manuais no próximo ciclo).
- Mudou porta, variável de ambiente ou dependência de uma app? Atualize juntos Dockerfile, chart (`templates/*.yaml`,
  `values.yaml`) e, se necessário, o bootstrap.
- Nova app/serviço: Dockerfile com contexto na raiz + entrada na matriz `images` do `deploy.yml` + templates no chart.
- Recursos (requests/limits) pensados para 4 GB: ao adicionar componentes, meça (`kubectl top pods -A`) e reavalie `instance_type`.
- Ao alterar o chart ou o bootstrap, valide localmente num k3s em container (`rancher/k3s`, sem Traefik) antes do `apply`.
