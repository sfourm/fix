variable "aws_region" {
  description = "Região da AWS."
  type        = string
  default     = "us-east-1"
}

variable "aws_profile" {
  description = "Perfil do AWS CLI (null = credenciais padrão: variáveis de ambiente, default profile ou SSO)."
  type        = string
  default     = null
}

variable "environment" {
  description = "Nome do ambiente (entra nos nomes, tags e no caminho dos parâmetros do SSM)."
  type        = string
  default     = "presentation"
}

variable "github_repository" {
  description = "Repositório do GitHub (owner/nome): publica imagens e chart no GHCR e é lido pelo Flux (GitOps)."
  type        = string
  default     = "sfourm/fix"
}

variable "github_branch" {
  description = "Branch que o Flux acompanha."
  type        = string
  default     = "main"
}

variable "gitops_path" {
  description = "Pasta do repositório com os manifests do ambiente (HelmRelease com chart e values)."
  type        = string
  default     = "infra/gitops/presentation"
}

# ---------- Máquina ----------

variable "instance_type" {
  description = "Tipo da EC2. t3.medium (2 vCPU, 4 GB) comporta o cluster inteiro (~2,1 GB em uso) com folga."
  type        = string
  default     = "t3.medium"
}

variable "root_volume_size" {
  description = "Disco raiz em GB (imagens, volumes do PostgreSQL e do Elasticsearch)."
  type        = number
  default     = 30
}

variable "swap_size_mb" {
  description = "Swap no nó (margem para picos de memória em instância pequena). 0 desativa."
  type        = number
  default     = 2048
}

variable "vpc_cidr" {
  description = "CIDR da VPC dedicada."
  type        = string
  default     = "10.60.0.0/16"
}

variable "admin_cidrs" {
  description = "CIDRs com acesso à API do Kubernetes (6443) e SSH (22). Vazio = fechado (acesso só via SSM Session Manager)."
  type        = list(string)
  default     = []
}

variable "ssh_key_name" {
  description = "Key pair existente para SSH (opcional; o acesso padrão é pelo SSM)."
  type        = string
  default     = null
}

# ---------- Kubernetes ----------

variable "k3s_version" {
  description = "Versão do k3s."
  type        = string
  default     = "v1.33.5+k3s1"
}

variable "helm_version" {
  description = "Versão do Helm instalada no nó."
  type        = string
  default     = "v3.19.1"
}

variable "ingress_nginx_chart_version" {
  description = "Versão do chart ingress-nginx."
  type        = string
  default     = "4.15.1"
}

variable "flux_chart_version" {
  description = "Versão do chart fluxcd-community/flux2 (Flux v2.9.x)."
  type        = string
  default     = "2.19.1"
}

variable "cert_manager_chart_version" {
  description = "Versão do chart cert-manager (usado só com enable_tls)."
  type        = string
  default     = "v1.21.2"
}

# ---------- Aplicação ----------

variable "domain_name" {
  description = "Host público do portal (outputs e DNS). O cluster usa o valor de infra/gitops/<ambiente>/fix.yaml: mantenha iguais."
  type        = string
  default     = ""
}

variable "grafana_domain_name" {
  description = "Host público do Grafana (outputs e DNS). Vazio = grafana.<domain_name>. O cluster usa infra/gitops/<ambiente>/fix.yaml."
  type        = string
  default     = ""
}

variable "enable_observability" {
  description = "Há Grafana no ambiente (outputs e DNS). Quem liga a stack no cluster é observability.enabled em infra/gitops/<ambiente>/fix.yaml."
  type        = bool
  default     = true
}

variable "enable_tls" {
  description = "URLs em https nos outputs. Quem liga o HTTPS no cluster é ingress.tls.enabled em infra/gitops/<ambiente>/fix.yaml."
  type        = bool
  default     = false
}

variable "letsencrypt_email" {
  description = "E-mail de contato do Let's Encrypt (avisos de expiração). Vazio = cadastro sem e-mail."
  type        = string
  default     = ""
}

variable "admin_email" {
  description = "E-mail do super administrador FIX criado no primeiro startup do core-service."
  type        = string
  default     = "admin@fix.local"
}

variable "ghcr_username" {
  description = "Usuário do GitHub dono do token de leitura do GHCR (necessário só se os pacotes forem privados)."
  type        = string
  default     = ""
}

variable "ghcr_pull_token" {
  description = "PAT (read:packages) para baixar imagens e chart privados do GHCR. Vazio = pacotes públicos."
  type        = string
  default     = ""
  sensitive   = true
}
