locals {
  name       = "fix-${var.environment}"
  ssm_prefix = "/fix/${var.environment}"

  # ghcr.io exige nomes em minúsculas.
  github_repository_lower = lower(var.github_repository)
  image_registry          = "ghcr.io/${local.github_repository_lower}"
  chart_ref               = "oci://ghcr.io/${local.github_repository_lower}/charts/fix"

  public_ip     = aws_eip.node.public_ip
  host          = var.domain_name != "" ? var.domain_name : "fix.${local.public_ip}.nip.io"
  grafana_host  = var.grafana_domain_name != "" ? var.grafana_domain_name : "grafana.${local.host}"
  use_ghcr_auth = var.ghcr_pull_token != ""
}

data "aws_availability_zones" "available" {
  state = "available"
}

# ---------- Rede: VPC pública mínima (sem NAT, para não gerar custo fixo) ----------

resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true
  tags                 = { Name = local.name }
}

resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id
  tags   = { Name = local.name }
}

resource "aws_subnet" "public" {
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(var.vpc_cidr, 8, 1)
  availability_zone = data.aws_availability_zones.available.names[0]
  tags              = { Name = "${local.name}-public" }
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = { Name = "${local.name}-public" }
}

resource "aws_route_table_association" "public" {
  subnet_id      = aws_subnet.public.id
  route_table_id = aws_route_table.public.id
}

# ---------- Security group: só HTTP/HTTPS abertos; API do Kubernetes e SSH apenas para admin_cidrs ----------

resource "aws_security_group" "node" {
  name        = "${local.name}-node"
  description = "No k3s da plataforma Fix" # a API do EC2 so aceita ASCII aqui
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "${local.name}-node" }
}

resource "aws_vpc_security_group_ingress_rule" "http" {
  security_group_id = aws_security_group.node.id
  description       = "HTTP (ingress-nginx e desafio HTTP-01 do Lets Encrypt)"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 80
  to_port           = 80
}

resource "aws_vpc_security_group_ingress_rule" "https" {
  security_group_id = aws_security_group.node.id
  description       = "HTTPS (ingress-nginx)"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 443
  to_port           = 443
}

resource "aws_vpc_security_group_ingress_rule" "kube_api" {
  for_each          = toset(var.admin_cidrs)
  security_group_id = aws_security_group.node.id
  description       = "API do Kubernetes (kubectl remoto)"
  cidr_ipv4         = each.value
  ip_protocol       = "tcp"
  from_port         = 6443
  to_port           = 6443
}

resource "aws_vpc_security_group_ingress_rule" "ssh" {
  for_each          = var.ssh_key_name == null ? toset([]) : toset(var.admin_cidrs)
  security_group_id = aws_security_group.node.id
  description       = "SSH"
  cidr_ipv4         = each.value
  ip_protocol       = "tcp"
  from_port         = 22
  to_port           = 22
}

resource "aws_vpc_security_group_egress_rule" "all" {
  security_group_id = aws_security_group.node.id
  description       = "Saida (imagens, charts, SSM, Lets Encrypt)"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
}

# ---------- Segredos da aplicação no SSM Parameter Store (lidos pelo nó no bootstrap) ----------

resource "random_password" "postgres" {
  length  = 32
  special = false
}

resource "random_password" "session_secret" {
  length  = 64
  special = false
}

resource "random_password" "admin" {
  length           = 20
  min_upper        = 2
  min_lower        = 2
  min_numeric      = 2
  min_special      = 1
  override_special = "@#%*-_"
}

resource "aws_ssm_parameter" "postgres_password" {
  name  = "${local.ssm_prefix}/postgres-password"
  type  = "SecureString"
  value = random_password.postgres.result
}

resource "aws_ssm_parameter" "session_secret" {
  name  = "${local.ssm_prefix}/session-secret"
  type  = "SecureString"
  value = random_password.session_secret.result
}

resource "aws_ssm_parameter" "admin_email" {
  name  = "${local.ssm_prefix}/admin-email"
  type  = "String"
  value = var.admin_email
}

resource "aws_ssm_parameter" "admin_password" {
  name  = "${local.ssm_prefix}/admin-password"
  type  = "SecureString"
  value = random_password.admin.result
}

resource "random_password" "grafana_admin" {
  length           = 20
  min_upper        = 2
  min_lower        = 2
  min_numeric      = 2
  min_special      = 1
  override_special = "@#%*-_"
}

resource "aws_ssm_parameter" "grafana_admin_password" {
  name  = "${local.ssm_prefix}/grafana-admin-password"
  type  = "SecureString"
  value = random_password.grafana_admin.result
}

# "-" = sem e-mail (o SSM não aceita valor vazio).
resource "aws_ssm_parameter" "letsencrypt_email" {
  name  = "${local.ssm_prefix}/letsencrypt-email"
  type  = "String"
  value = var.letsencrypt_email != "" ? var.letsencrypt_email : "-"
}

# Valores do chart para este ambiente: o fix-deploy lê a cada deploy (mudar aqui não recria a instância).
resource "aws_ssm_parameter" "helm_values" {
  name = "${local.ssm_prefix}/helm-values"
  type = "String"
  value = templatefile("${path.module}/templates/values.yaml.tftpl", {
    image_registry       = local.image_registry
    host                 = local.host
    grafana_host         = local.grafana_host
    enable_tls           = var.enable_tls
    enable_observability = var.enable_observability
    use_ghcr_auth        = local.use_ghcr_auth
  })
}

resource "aws_ssm_parameter" "ghcr_username" {
  count = local.use_ghcr_auth ? 1 : 0
  name  = "${local.ssm_prefix}/ghcr-username"
  type  = "String"
  value = var.ghcr_username
}

resource "aws_ssm_parameter" "ghcr_token" {
  count = local.use_ghcr_auth ? 1 : 0
  name  = "${local.ssm_prefix}/ghcr-token"
  type  = "SecureString"
  value = var.ghcr_pull_token
}

# kubeconfig do cluster para o kubectl local: criado vazio aqui (para o destroy removê-lo) e preenchido pelo bootstrap do nó.
resource "aws_ssm_parameter" "kubeconfig" {
  name        = "${local.ssm_prefix}/kubeconfig"
  description = "kubeconfig do k3s (server = IP elastico). Baixe com infra/scripts/kubeconfig.ps1 ou kubeconfig.sh."
  type        = "SecureString"
  tier        = "Intelligent-Tiering"
  value       = "pendente: preenchido pelo bootstrap do nó"

  lifecycle {
    ignore_changes = [value, tier]
  }
}
