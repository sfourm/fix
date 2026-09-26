output "app_url" {
  description = "URL da aplicação (disponível após o primeiro deploy pelo GitHub Actions)."
  value       = "${var.enable_tls ? "https" : "http"}://${local.host}"
}

output "grafana_url" {
  description = "Grafana (usuário admin). A UI do Jaeger fica em <grafana_url>/jaeger."
  value       = var.enable_observability ? "${var.enable_tls ? "https" : "http"}://${local.grafana_host}" : null
}

output "grafana_admin_password" {
  description = "Senha do admin do Grafana (terraform output -raw grafana_admin_password)."
  value       = random_password.grafana_admin.result
  sensitive   = true
}

output "dns_records" {
  description = "Registros A a criar no DNS do domínio (todos apontam para o IP elástico)."
  value       = { for h in compact([local.host, var.enable_observability ? local.grafana_host : ""]) : h => aws_eip.node.public_ip }
}

output "files_bucket" {
  description = "Bucket S3 dos arquivos da tela de Uploads (storage-service)."
  value       = aws_s3_bucket.files.bucket
}

output "public_ip" {
  description = "IP elástico do nó."
  value       = aws_eip.node.public_ip
}

output "instance_id" {
  description = "ID da EC2 (SSM Session Manager e túnel do kubectl)."
  value       = aws_instance.node.id
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

output "gitops" {
  description = "De onde o cluster lê a aplicação (Flux)."
  value       = "https://github.com/${var.github_repository}/tree/${var.github_branch}/${var.gitops_path}"
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
