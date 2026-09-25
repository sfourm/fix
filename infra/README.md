# Infraestrutura: cluster k3s na AWS (apresentação)

Um nó EC2 pequeno com **k3s** (Kubernetes), **Helm**, **ingress-nginx**, **cert-manager** (HTTPS com Let's Encrypt) e
**Flux** (GitOps), com a stack de observabilidade (OTel Collector, Prometheus, Jaeger e **Grafana**) no próprio cluster.
A plataforma Fix (core-service, BFF, web, PostgreSQL, Redis, Elasticsearch) é instalada pelo chart `helm/fix`.

**Como uma mudança chega ao cluster (GitOps):** o GitHub Actions testa e publica imagens e chart no **GHCR** (Packages do
GitHub); o **Flux**, rodando no cluster, lê este repositório (`gitops/presentation`) e o GHCR e aplica sozinho. O GitHub
não tem credenciais da AWS e não acessa o cluster. O cluster também fica disponível no seu `kubectl`.

```
infra/
├── terraform/          # VPC, EC2 + EIP, security group, IAM do nó, segredos no SSM, bootstrap (k3s, ingress, cert-manager, Flux)
├── helm/fix/           # chart da plataforma
├── gitops/
│   ├── presentation/   # o que o cluster aplica: HelmRelease (versão do chart + values do ambiente)
│   └── flux/           # liga o Flux do cluster a este repositório (aplicado uma vez)
└── scripts/            # kubeconfig.ps1 / kubeconfig.sh: adicionam o cluster ao kubectl local
```

## Onde fica cada configuração

| O quê | Onde | Muda como |
| --- | --- | --- |
| Código, imagens, chart | Repositório → GHCR (pipeline) | Push na `main` |
| Versão do chart e values do ambiente (domínios, HTTPS, observabilidade) | `infra/gitops/presentation/fix.yaml` | Push na `main` (o Flux aplica em ~2 min) |
| Dashboards do Grafana | `observability/grafana/dashboards/*.json` | Push na `main` (entram no próximo chart) |
| Segredos (PostgreSQL, sessão, super admin, Grafana, e-mail do Let's Encrypt) | SSM Parameter Store (Terraform) | `terraform apply` + `sudo fix-sync` no nó |
| Máquina, rede, DNS de saída (outputs) | `infra/terraform` | `terraform apply` |

## Custo e dimensionamento

`t3.medium` (2 vCPU, 4 GB) + 30 GB gp3 + IP elástico ≈ **US$ 35/mês** ligado (us-east-1). Com tudo ligado (aplicação,
observabilidade, cert-manager e Flux) o cluster usa ~2,9 GB; o nó tem 2 GB de swap de margem. Se aparecer pod reiniciando por
memória (`OOMKilled`), suba para `t3.large`. Depois da apresentação: `terraform destroy`.

## Pré-requisitos

- Terraform ≥ 1.6, AWS CLI v2 com credenciais de uma identidade que possa criar VPC, EC2, IAM e parâmetros do SSM.
  Teste com `aws sts get-caller-identity`.
- `kubectl` e, para o modo túnel, o [Session Manager plugin](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-working-with-install-plugin.html).
- Repositório e pacotes do GHCR **públicos** (padrão quando o repositório é público). Se forem privados, veja
  "Pacotes privados" abaixo.

## 1. Criar a infraestrutura

```bash
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars   # ajuste: admin_cidrs (seu IP/32), domínios, e-mail do Let's Encrypt
terraform init
terraform apply
terraform output dns_records                   # cadastre os registros A no DNS do domínio
```

O bootstrap do nó leva ~5 minutos (k3s, Helm, ingress-nginx, cert-manager, Flux, segredos). Em seguida o Flux instala a
aplicação a partir do repositório. Acompanhe:

```bash
aws ssm start-session --target $(terraform output -raw instance_id)
sudo tail -f /var/log/fix-bootstrap.log     # termina com "bootstrap concluído"
```

## 2. Deploy (GitOps)

Não há nada para configurar no GitHub. Todo push na `main` roda `.github/workflows/deploy.yml`:

1. **CI**: testes do core, BFF e web; lint do chart; validate do Terraform.
2. **Imagens** `ghcr.io/<owner>/<repo>/{core-service,bff,web}:<sha>` (e `:latest`).
3. **Chart** `oci://ghcr.io/<owner>/<repo>/charts/fix`, versão `0.1.<run_number>`, com `appVersion = <sha>` (as imagens do
   mesmo build).

O Flux verifica o GHCR e o repositório a cada 1–2 minutos: encontrou chart novo ou mudança em `infra/gitops/presentation`,
faz `helm upgrade`. Se o upgrade falhar duas vezes, volta para a versão anterior.

```bash
kubectl -n fix get helmrelease fix                      # versão aplicada e status
kubectl -n flux-system get gitrepository,kustomization  # leitura do repositório
kubectl -n fix describe helmrelease fix                 # detalhes em caso de erro
```

Para fixar uma versão (ou voltar para uma anterior), troque `version: ">=0.1.0"` por uma versão exata em
`infra/gitops/presentation/fix.yaml` e faça push.

**Pacotes privados**: se os pacotes do GHCR forem privados, informe `ghcr_username` e `ghcr_pull_token` (PAT com
`read:packages`) no `terraform.tfvars`, rode `terraform apply` e `sudo fix-sync` no nó; use `image.pullSecrets: [{name: ghcr-pull}]`
nos values e adicione `secretRef` no `HelmRepository`.

## 3. kubectl

```powershell
# Windows (PowerShell) — acesso direto (seu IP em admin_cidrs)
./infra/scripts/kubeconfig.ps1
# sem porta 6443 aberta: túnel SSM
./infra/scripts/kubeconfig.ps1 -Tunnel -InstanceId (terraform -chdir=infra/terraform output -raw instance_id)
```

```bash
./infra/scripts/kubeconfig.sh            # ou --tunnel --instance-id <id>
```

O script lê o kubeconfig que o nó publicou no SSM (`/fix/<ambiente>/kubeconfig`, SecureString), mescla em `~/.kube/config`
(com backup `.bak`) e seleciona o contexto **`fix-presentation`**. Se a instância for recriada, rode o script de novo (o
certificado do cluster muda).

No modo túnel, mantenha aberto em outro terminal:
`aws ssm start-session --target <id> --document-name AWS-StartPortForwardingSession --parameters portNumber=6443,localPortNumber=16443`.

## 4. Acessar a aplicação

- Portal: `terraform output app_url` (ex.: `https://fix.webpassos.com.br`).
- Grafana: `terraform output grafana_url` (ex.: `https://grafana.fix.webpassos.com.br`), usuário `admin`, senha em
  `terraform output -raw grafana_admin_password`. UI do Jaeger em `<grafana_url>/jaeger` (exige login no Grafana).
- DNS: `terraform output dns_records`. O certificado do Let's Encrypt sai sozinho depois que o DNS aponta para o IP.
- Super administrador FIX: `terraform output super_admin_email` e `terraform output -raw super_admin_password`.

Os domínios aparecem em dois lugares: `infra/gitops/presentation/fix.yaml` (o que o cluster usa) e `terraform.tfvars`
(outputs e DNS). Mantenha iguais.

## Segurança

- Abertas ao mundo: só 80/443 (ingress-nginx). 6443 (API do Kubernetes) e 22 apenas para `admin_cidrs`; vazio = fechadas.
- O GitHub não tem acesso à AWS nem ao cluster: o cluster **puxa** do repositório e do GHCR (públicos, só leitura).
- Segredos ficam no SSM Parameter Store (SecureString); o nó lê com a própria role (`fix-sync`) e cria o Secret
  `fix-secrets`. Nada sensível fica no repositório. **O estado do Terraform contém os segredos**: para uso em equipe, use
  backend S3 criptografado (bloco comentado em `versions.tf`).
- Push na `main` vai para produção: proteja a branch (Settings › Branches) se mais gente tiver acesso de escrita.
- IMDSv2 obrigatório com hop limit 1 (pods não alcançam as credenciais do nó); disco criptografado.
- O core-service não é exposto: só o BFF fala com ele, dentro do cluster.

## Operação

| Ação | Como |
| --- | --- |
| Ver o que está aplicado | `kubectl -n fix get helmrelease fix` e `helm -n fix history fix` |
| Voltar uma versão | Fixe a versão anterior em `infra/gitops/presentation/fix.yaml` e faça push |
| Trocar domínio, HTTPS ou observabilidade | Edite `infra/gitops/presentation/fix.yaml` (e `terraform.tfvars` para os outputs) e faça push |
| Trocar um segredo | Altere o parâmetro no SSM (ou `terraform apply`) e rode `sudo fix-sync` no nó. As senhas do PostgreSQL e do admin do Grafana só valem na criação dos volumes |
| Editar dashboards do Grafana | Edite `observability/grafana/dashboards/*.json` e faça push |
| Forçar o Flux a ler agora | `kubectl -n flux-system annotate gitrepository fix reconcile.fluxcd.io/requestedAt="$(date +%s)" --overwrite` |
| Mudar o bootstrap do nó | Edite o template; só vale para instância nova: `terraform apply -replace=aws_instance.node` (perde os dados do cluster) |
| Desligar tudo | `terraform destroy` |
