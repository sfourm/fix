# Amazon Linux 2023: já traz o SSM Agent e o AWS CLI (usados no bootstrap e no deploy).
data "aws_ssm_parameter" "al2023" {
  name = "/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64"
}

# IP fixo: o host nip.io, o certificado da API do Kubernetes (tls-san) e o Ingress dependem dele.
resource "aws_eip" "node" {
  domain = "vpc"
  tags   = { Name = local.name }
}

resource "aws_instance" "node" {
  ami                    = data.aws_ssm_parameter.al2023.value
  instance_type          = var.instance_type
  subnet_id              = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.node.id]
  iam_instance_profile   = aws_iam_instance_profile.node.name
  key_name               = var.ssh_key_name

  # IP público temporário até a associação do EIP (o bootstrap precisa de internet desde o boot).
  associate_public_ip_address = true

  root_block_device {
    volume_type           = "gp3"
    volume_size           = var.root_volume_size
    encrypted             = true
    delete_on_termination = true
  }

  metadata_options {
    http_tokens                 = "required" # IMDSv2
    http_put_response_hop_limit = 1          # pods não alcançam as credenciais do nó
  }

  user_data_replace_on_change = true
  # Só a base do cluster; domínios, valores e segredos vêm do SSM no deploy (mudá-los não recria a instância).
  user_data = templatefile("${path.module}/templates/user-data.sh.tftpl", {
    aws_region                  = var.aws_region
    ssm_prefix                  = local.ssm_prefix
    public_ip                   = local.public_ip
    swap_size_mb                = var.swap_size_mb
    k3s_version                 = var.k3s_version
    helm_version                = var.helm_version
    ingress_nginx_chart_version = var.ingress_nginx_chart_version
    cert_manager_chart_version  = var.cert_manager_chart_version
    deploy_script = templatefile("${path.module}/templates/fix-deploy.sh.tftpl", {
      aws_region = var.aws_region
      ssm_prefix = local.ssm_prefix
    })
  })

  tags = { Name = local.name }

  lifecycle {
    precondition {
      condition     = var.ghcr_pull_token == "" || var.ghcr_username != ""
      error_message = "ghcr_pull_token exige ghcr_username."
    }
  }

  depends_on = [
    aws_route_table_association.public,
    aws_ssm_parameter.postgres_password,
    aws_ssm_parameter.session_secret,
    aws_ssm_parameter.admin_email,
    aws_ssm_parameter.admin_password,
    aws_ssm_parameter.grafana_admin_password,
    aws_ssm_parameter.letsencrypt_email,
    aws_ssm_parameter.helm_values,
    aws_ssm_parameter.ghcr_username,
    aws_ssm_parameter.ghcr_token,
    aws_ssm_parameter.kubeconfig,
  ]
}

resource "aws_eip_association" "node" {
  instance_id   = aws_instance.node.id
  allocation_id = aws_eip.node.id
}
