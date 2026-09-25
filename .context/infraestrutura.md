# Infraestrutura e entrega (`infra/`, `.github/`, Dockerfiles)

Ambiente de **apresentação**: um único nó EC2 com **k3s**, dimensionado para custo baixo. Passo a passo operacional em
[`infra/README.md`](../infra/README.md); aqui ficam a arquitetura e as regras para quem altera a infraestrutura.

## Visão geral

```
GitHub (push main) ──CI──▶ imagens + chart ──▶ GHCR (ghcr.io/<owner>/<repo>/…)
        │
        └─OIDC──▶ role AWS (só ssm:SendCommand nesta EC2) ──SSM Run Command──▶ fix-deploy ──helm upgrade──▶ k3s
                                                                                                         │
Internet ──80/443──▶ EC2 (EIP) ─▶ ServiceLB do k3s ─▶ ingress-nginx ─▶ /api → BFF · / → web ◀────────────┘
                                                                        BFF → core-service (gRPC) → PostgreSQL
                                                                        BFF → Redis · Elasticsearch
Seu kubectl ──6443 (admin_cidrs) ou túnel SSM──▶ API do k3s (contexto fix-<ambiente>)
```

| Peça | Onde | Papel |
| --- | --- | --- |
| Dockerfiles | `backend/core-service/Dockerfile`, `frontend/bff/Dockerfile`, `frontend/web/Dockerfile` | Build a partir da **raiz** do repositório (`.dockerignore` na raiz); core e BFF precisam de `protos/` |
| Chart Helm | `infra/helm/fix` | Plataforma inteira: 3 apps + PostgreSQL, Redis, Elasticsearch + Ingress |
| Terraform | `infra/terraform` | VPC mínima, EC2 + EIP, SG, IAM (nó e GitHub OIDC), segredos no SSM, bootstrap (user-data) |
| Scripts | `infra/scripts/kubeconfig.{ps1,sh}` | Colocam o cluster no `kubectl` local |
| Pipelines | `.github/workflows/ci.yml`, `deploy.yml` | CI em PR; na `main`: CI → imagens → chart → deploy |

## Imagens

| Imagem | Base | Porta | Observações |
| --- | --- | --- | --- |
| `core-service` | `dotnet/sdk:10.0` → `dotnet/aspnet:10.0` | 5098 (h2c) | Usuário não-root `$APP_UID`; `ASPNETCORE_ENVIRONMENT=Production` |
| `bff` | `node:22-alpine` | 3000 | `npm prune --omit=dev`; protos em `/app/protos`; roda com `--import ./dist/instrumentation.js` |
| `web` | `node:22-alpine` → `nginx-unprivileged` | 8080 | SPA com fallback para `index.html`, `/healthz`; chama a API em `/api` (mesmo origin) |

## Chart `fix`

- Nomes: `<release>-<componente>` (`fix-core-service`, `fix-bff`, `fix-web`, `fix-postgres`, `fix-redis`, `fix-elasticsearch`).
- Imagem: `<image.registry>/<componente>:<tag>`, com `tag` = `image.tag` ou, por padrão, a **appVersion** do chart. O pipeline
  publica o chart com `appVersion=<sha>`: cada versão do chart instala as imagens geradas no mesmo build (Packages do GitHub).
- Segredos vêm do Secret `fix-secrets` (`postgres-password`, `session-secret`, `admin-email`, `admin-password`), criado pelo
  bootstrap a partir do SSM. `secrets.create=true` só para testes locais.
- Valores do ambiente (host, TLS, registry, pull secret) ficam no nó em `/etc/fix/values.yaml`, gerados pelo Terraform.
- Ingress `nginx`: `/api` → BFF, `/` → web. O core-service **não** é exposto. Com `ingress.tls.enabled`, o cert-manager emite
  o certificado (ClusterIssuer `letsencrypt`).
- core-service roda as migrations no startup (`Database__MigrateOnStartup=true`) e cria o super administrador (`Seed__*`);
  `startupProbe` dá até 3 min para isso. Init containers aguardam PostgreSQL (core) e core + Elasticsearch (BFF).
- Telemetria desligada por padrão (`telemetry.enabled=false`): a stack de `observability/` não roda no cluster.
- Segurança dos pods: `runAsNonRoot`, sem escalonamento de privilégio, capabilities removidas; init containers como 65534.
- Recursos pequenos por padrão (ver `values.yaml`); Elasticsearch com heap de 384 MB.

## Nó (bootstrap em `templates/user-data.sh.tftpl`)

Amazon Linux 2023 (SSM Agent e AWS CLI nativos) → swap 2 GB e `vm.max_map_count` → k3s (`--disable traefik`,
`--tls-san <EIP>`) → Helm → ingress-nginx (Service LoadBalancer publicado nas portas 80/443 pelo ServiceLB do k3s) →
cert-manager (opcional) → namespace `fix` + Secret `fix-secrets` → `/etc/fix/values.yaml` + `/usr/local/bin/fix-deploy` →
kubeconfig publicado no SSM (`/fix/<ambiente>/kubeconfig`, contexto `fix-<ambiente>`, server = EIP) → marca
`/var/lib/fix/bootstrap-done`. Log em `/var/log/fix-bootstrap.log`.

Versões fixadas em variáveis: k3s `v1.33.5+k3s1`, Helm `v3.19.1`, ingress-nginx `4.15.1`, cert-manager `v1.21.2`.

## Deploy (`fix-deploy`)

`fix-deploy <oci://ghcr.io/...> <x.y.z> <tag>` valida as entradas (regex), renova o login no GHCR se houver token,
e roda `helm upgrade --install fix … --values /etc/fix/values.yaml --set image.tag=<tag> --wait --atomic`.
O workflow acompanha o `ssm get-command-invocation` e publica a saída no resumo do job.

## Regras

- Segredo nunca vai para o repositório, o chart ou o workflow: fica no SSM (e no estado do Terraform — proteja o estado).
- O GitHub não recebe chaves da AWS: só OIDC, com role restrita à instância e ao documento `AWS-RunShellScript`.
- Mudou porta, variável de ambiente ou dependência de uma app? Atualize juntos Dockerfile, chart (`templates/*.yaml`,
  `values.yaml`) e, se necessário, o bootstrap.
- Nova app/serviço: Dockerfile com contexto na raiz + entrada na matriz `images` do `deploy.yml` + templates no chart.
- Recursos (requests/limits) pensados para 4 GB: ao adicionar componentes, meça (`kubectl top pods -A`) e reavalie `instance_type`.
- Ao alterar o chart ou o bootstrap, valide localmente num k3s em container (`rancher/k3s`, sem Traefik) antes do `apply`.
