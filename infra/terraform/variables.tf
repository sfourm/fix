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
  description = "Repositório do GitHub (owner/nome) que publica as imagens no GHCR e faz o deploy."
  type        = string
  default     = "sfourm/fix"
}

variable "github_deploy_refs" {
  description = "Refs do GitHub que podem assumir a role de deploy (claim sub do OIDC)."
  type        = list(string)
  default     = ["ref:refs/heads/main"]
}

variable "create_github_oidc_provider" {
  description = "Cria o OIDC provider do GitHub na conta. Use false se ele já existir (só pode haver um por conta)."
  type        = bool
  default     = true
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

variable "cert_manager_chart_version" {
  description = "Versão do chart cert-manager (usado só com enable_tls)."
  type        = string
  default     = "v1.21.2"
}

# ---------- Aplicação ----------

variable "domain_name" {
  description = "Host público da aplicação. Vazio = fix.<ip-elástico>.nip.io (DNS mágico, sem configurar domínio)."
  type        = string
  default     = ""
}

variable "enable_tls" {
  description = "Instala o cert-manager e emite certificado Let's Encrypt para o host (HTTPS)."
  type        = bool
  default     = false
}

variable "letsencrypt_email" {
  description = "E-mail de contato do Let's Encrypt (obrigatório com enable_tls)."
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
