<#
.SYNOPSIS
  Adiciona o cluster k3s da Fix ao kubectl local (contexto fix-<ambiente>).

.DESCRIPTION
  Baixa o kubeconfig publicado pelo nó no SSM Parameter Store (/fix/<ambiente>/kubeconfig), mescla em ~/.kube/config
  e seleciona o contexto. Dois modos de acesso à API do Kubernetes (6443):
    - direto: o seu IP precisa estar em admin_cidrs no Terraform;
    - -Tunnel: sem porta aberta, por port forwarding do SSM (requer o Session Manager plugin do AWS CLI).

.EXAMPLE
  ./infra/scripts/kubeconfig.ps1                      # acesso direto pelo IP elástico
  ./infra/scripts/kubeconfig.ps1 -Tunnel              # via túnel SSM em https://127.0.0.1:16443
  ./infra/scripts/kubeconfig.ps1 -Profile minha-conta -Region us-east-1
#>
[CmdletBinding()]
param(
  [string] $Environment = 'presentation',
  [string] $Region = 'us-east-1',
  [string] $Profile,
  [switch] $Tunnel,
  [int] $LocalPort = 16443,
  # ID da EC2 para o túnel (padrão: busca pela tag Name=fix-<ambiente>; ou terraform output -raw instance_id).
  [string] $InstanceId,
  # Destino da mesclagem (padrão ~/.kube/config).
  [string] $KubeconfigPath = (Join-Path $HOME '.kube/config'),
  # Usa um kubeconfig já baixado em vez de ler do SSM.
  [string] $SourceFile
)

$ErrorActionPreference = 'Stop'
$name = "fix-$Environment"
$awsArgs = @('--region', $Region)
if ($Profile) { $awsArgs += @('--profile', $Profile) }

if ($SourceFile) {
  $content = Get-Content -Raw $SourceFile
} else {
  $content = aws ssm get-parameter @awsArgs --name "/fix/$Environment/kubeconfig" --with-decryption --query Parameter.Value --output text
  if ($LASTEXITCODE -ne 0) { throw 'Não foi possível ler o kubeconfig no SSM (credenciais/região corretas? o terraform apply terminou?).' }
  $content = $content -join "`n"
}
if ($content -notmatch 'apiVersion:\s*v1') { throw "O kubeconfig ainda não foi publicado pelo nó (valor atual: '$content'). Aguarde o bootstrap terminar (~5 min)." }

if ($Tunnel) {
  $content = $content -replace 'server:\s*https://[^\s]+:6443', "server: https://127.0.0.1:$LocalPort"
}

$kubeDir = Split-Path -Parent $KubeconfigPath
New-Item -ItemType Directory -Force -Path $kubeDir | Out-Null
# Cópia intermediária fora de ~/.kube: ferramentas como Freelens/Lens registram cada arquivo da pasta como cluster.
$clusterFile = Join-Path ([IO.Path]::GetTempPath()) "$name-$PID.yaml"
[IO.File]::WriteAllText($clusterFile, $content, [Text.UTF8Encoding]::new($false))

# Mescla: o arquivo novo vem primeiro, então suas entradas (mesmo nome) substituem as antigas.
$sources = @($clusterFile)
if (Test-Path $KubeconfigPath) {
  Copy-Item $KubeconfigPath "$KubeconfigPath.bak" -Force
  $sources += $KubeconfigPath
}
$previous = $env:KUBECONFIG
try {
  $env:KUBECONFIG = $sources -join [IO.Path]::PathSeparator
  $merged = kubectl config view --flatten
  if ($LASTEXITCODE -ne 0) { throw 'kubectl config view falhou.' }
} finally {
  $env:KUBECONFIG = $previous
  Remove-Item $clusterFile -Force -ErrorAction SilentlyContinue
}
[IO.File]::WriteAllText($KubeconfigPath, (($merged -join "`n") + "`n"), [Text.UTF8Encoding]::new($false))
kubectl config use-context $name --kubeconfig $KubeconfigPath | Out-Null

Write-Host "Contexto '$name' adicionado a $KubeconfigPath e selecionado (backup em $KubeconfigPath.bak)."
if ($Tunnel) {
  $instance = $InstanceId
  if (-not $instance) {
    $instance = aws ec2 describe-instances @awsArgs --filters "Name=tag:Name,Values=$name" "Name=instance-state-name,Values=running" --query 'Reservations[0].Instances[0].InstanceId' --output text 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $instance -or $instance -eq 'None') { $instance = '<instance_id: terraform output -raw instance_id>' }
  }
  Write-Host ''
  Write-Host 'Abra o túnel em outro terminal e mantenha-o aberto enquanto usar o kubectl:'
  Write-Host "  aws ssm start-session $($awsArgs -join ' ') --target $instance --document-name AWS-StartPortForwardingSession --parameters portNumber=6443,localPortNumber=$LocalPort"
} else {
  Write-Host 'Acesso direto: o seu IP precisa estar em admin_cidrs no Terraform. Teste com: kubectl get nodes'
}
