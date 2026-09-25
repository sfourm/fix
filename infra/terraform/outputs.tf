output "app_url" {
  description = "URL da aplicação (disponível após o primeiro deploy pelo GitHub Actions)."
  value       = "${var.enable_tls ? "https" : "http"}://${local.host}"
}

output "public_ip" {
  description = "IP elástico do nó."
  value       = aws_eip.node.public_ip
}

output "instance_id" {
  description = "ID da EC2 (variável EC2_INSTANCE_ID no GitHub)."
  value       = aws_instance.node.id
}

output "github_deploy_role_arn" {
  description = "Role assumida pelo GitHub Actions via OIDC (variável AWS_DEPLOY_ROLE_ARN no GitHub)."
  value       = aws_iam_role.github_deploy.arn
}

output "chart_ref" {
  description = "Referência OCI do chart publicado pelo pipeline."
  value       = local.chart_ref
}

output "super_admin_email" {
  value = var.admin_email
}

output "super_admin_password" {
  description = "Senha do super administrador FIX (terraform output -raw super_admin_password)."
  value       = random_password.admin.result
  sensitive   = true
}

output "github_variables" {
  description = "Comandos para configurar as variáveis do repositório no GitHub (gh CLI)."
  value       = <<-EOT
    gh variable set AWS_REGION --repo ${var.github_repository} --body ${var.aws_region}
    gh variable set AWS_DEPLOY_ROLE_ARN --repo ${var.github_repository} --body ${aws_iam_role.github_deploy.arn}
    gh variable set EC2_INSTANCE_ID --repo ${var.github_repository} --body ${aws_instance.node.id}
  EOT
}

output "ssm_session_command" {
  description = "Terminal no nó sem SSH (Session Manager plugin). Log do bootstrap: sudo tail -f /var/log/fix-bootstrap.log"
  value       = "aws ssm start-session --region ${var.aws_region} --target ${aws_instance.node.id}"
}

output "kubectl_setup" {
  description = "Adiciona o cluster ao kubectl local (contexto fix-<ambiente>), após o bootstrap do nó (~5 min)."
  value       = <<-EOT
    # Acesso direto (seu IP em admin_cidrs):
    ./infra/scripts/kubeconfig.ps1 -Environment ${var.environment} -Region ${var.aws_region}
    ./infra/scripts/kubeconfig.sh --env ${var.environment} --region ${var.aws_region}
    # Sem porta aberta, via túnel SSM (Session Manager plugin):
    ./infra/scripts/kubeconfig.ps1 -Environment ${var.environment} -Region ${var.aws_region} -Tunnel -InstanceId ${aws_instance.node.id}
    aws ssm start-session --region ${var.aws_region} --target ${aws_instance.node.id} --document-name AWS-StartPortForwardingSession --parameters portNumber=6443,localPortNumber=16443
  EOT
}
