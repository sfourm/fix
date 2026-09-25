# Infraestrutura: cluster k3s na AWS (apresentação)

Um nó EC2 pequeno com **k3s** (Kubernetes), **Helm**, **ingress-nginx** e, opcionalmente, **cert-manager** (HTTPS).
A plataforma Fix (core-service, BFF, web, PostgreSQL, Redis, Elasticsearch) é instalada pelo chart `helm/fix`.
Imagens e chart ficam no **GHCR**; o **GitHub Actions** publica e faz o deploy. O cluster fica disponível no seu `kubectl`.

```
infra/
├── terraform/   # VPC, EC2 + EIP, security group, IAM (nó e GitHub OIDC), segredos no SSM, bootstrap do k3s
├── helm/fix/    # chart da plataforma
└── scripts/     # kubeconfig.ps1 / kubeconfig.sh: adicionam o cluster ao kubectl local
```

## Custo e dimensionamento

`t3.medium` (2 vCPU, 4 GB) + 30 GB gp3 + IP elástico ≈ **US$ 35/mês** ligado (us-east-1). Medido em teste: o cluster inteiro
usa ~2,1 GB (Elasticsearch ~0,8 GB, core ~0,2 GB, ingress ~0,1 GB, BFF ~0,07 GB). O nó tem 2 GB de swap de margem.
Depois da apresentação: `terraform destroy`.

## Pré-requisitos

- Terraform ≥ 1.6, AWS CLI v2 com credenciais de uma identidade que possa criar VPC, EC2, IAM (roles, OIDC provider) e
  parâmetros do SSM. Teste com `aws sts get-caller-identity`.
- `kubectl` e, para o modo túnel, o [Session Manager plugin](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-working-with-install-plugin.html).
- Repositório no GitHub com Actions habilitado (`gh` CLI ajuda a configurar as variáveis).

## 1. Criar a infraestrutura

```bash
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars   # ajuste: admin_cidrs (seu IP/32), enable_tls, etc.
terraform init
terraform apply
```

O bootstrap do nó leva ~5 minutos (k3s, Helm, ingress-nginx, segredos). Acompanhe:

```bash
aws ssm start-session --target $(terraform output -raw instance_id)
sudo tail -f /var/log/fix-bootstrap.log     # termina com "bootstrap concluído"
```

## 2. Configurar o GitHub

```bash
terraform output -raw github_variables | bash     # cria AWS_REGION, AWS_DEPLOY_ROLE_ARN e EC2_INSTANCE_ID no repositório
```

Sem `gh`: Settings › Secrets and variables › Actions › Variables, com os valores de `terraform output`.

Nenhuma chave da AWS vai para o GitHub: o workflow assume a role `fix-<ambiente>-github-deploy` por **OIDC**, e essa role só
pode executar comandos SSM **nesta** instância.

## 3. Deploy

Todo push na `main` roda `.github/workflows/deploy.yml`:

1. **CI** (testes do core, BFF e web; lint do chart; validate do Terraform).
2. **Imagens** `ghcr.io/<owner>/<repo>/{core-service,bff,web}:<sha>` (e `:latest`).
3. **Chart** `oci://ghcr.io/<owner>/<repo>/charts/fix`, versão `0.1.<run_number>`.
4. **Deploy**: `aws ssm send-command` executa no nó `fix-deploy <chart> <versão> <sha>` →
   `helm upgrade --install --atomic` (falhou, volta para a versão anterior).

Também pode ser disparado manualmente (Actions › Deploy › Run workflow).

**Visibilidade dos pacotes**: o GHCR cria os pacotes como privados. Escolha uma opção:
- torne `core-service`, `bff`, `web` e `charts/fix` **públicos** (GitHub › Packages › Package settings), ou
- informe `ghcr_username` e `ghcr_pull_token` (PAT com `read:packages`) no `terraform.tfvars` e rode `terraform apply`:
  o nó faz login no GHCR e o chart passa a usar o secret `ghcr-pull`.

## 4. kubectl

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
(com backup `.bak`) e seleciona o contexto **`fix-presentation`**:

```bash
kubectl get nodes
kubectl -n fix get pods
kubectl -n fix logs deploy/fix-core-service
kubectl config use-context fix-presentation   # voltar ao cluster depois de trocar de contexto
```

No modo túnel, mantenha aberto em outro terminal:
`aws ssm start-session --target <id> --document-name AWS-StartPortForwardingSession --parameters portNumber=6443,localPortNumber=16443`.

## 5. Acessar a aplicação

- URL: `terraform output app_url` → `http://fix.<ip>.nip.io` (ou o `domain_name`; `https` com `enable_tls`).
- Super administrador FIX: `terraform output super_admin_email` e `terraform output -raw super_admin_password`.

## Segurança

- Abertas ao mundo: só 80/443 (ingress-nginx). 6443 (API do Kubernetes) e 22 apenas para `admin_cidrs`; vazio = fechadas.
- Segredos (PostgreSQL, sessão do BFF, senha do super admin, token do GHCR, kubeconfig) ficam no SSM Parameter Store
  (SecureString); o nó lê com a própria role e cria o Secret `fix-secrets`. **O estado do Terraform também os contém**:
  para uso em equipe, use backend S3 criptografado (bloco comentado em `versions.tf`).
- IMDSv2 obrigatório com hop limit 1 (pods não alcançam as credenciais do nó); disco criptografado.
- O core-service não é exposto: só o BFF fala com ele, dentro do cluster.

## Operação

| Ação | Como |
| --- | --- |
| Ver o que rodou no deploy | Resumo do job no GitHub Actions, ou `helm -n fix history fix` |
| Voltar uma versão | `helm -n fix rollback fix <revisão>` |
| Trocar um segredo | Altere o parâmetro no SSM, recrie o Secret `fix-secrets` no nó (mesmo comando do bootstrap) e `kubectl -n fix rollout restart deploy`. A senha do PostgreSQL só vale na criação do volume |
| Recriar o nó do zero | `terraform apply -replace=aws_instance.node` (os dados dos volumes do k3s são perdidos) e rodar o deploy de novo |
| Desligar tudo | `terraform destroy` |

## Testado localmente

O chart e o fluxo de deploy foram validados em um k3s local (container `rancher/k3s`) configurado como o nó:
sem Traefik, ingress-nginx pelo Helm, chart publicado/baixado via OCI, `fix-deploy` (validação de entradas) e
`kubeconfig.ps1`/`.sh` (mesclagem de contexto). O `terraform validate` e o `actionlint` passam; o `apply` na AWS não foi executado.
