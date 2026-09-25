#!/usr/bin/env bash
# Adiciona o cluster k3s da Fix ao kubectl local (contexto fix-<ambiente>).
# Baixa o kubeconfig publicado pelo nó no SSM (/fix/<ambiente>/kubeconfig), mescla em ~/.kube/config e seleciona o contexto.
#
#   ./infra/scripts/kubeconfig.sh                         # acesso direto (seu IP em admin_cidrs no Terraform)
#   ./infra/scripts/kubeconfig.sh --tunnel                # via túnel SSM em https://127.0.0.1:16443 (sem porta aberta)
#   ./infra/scripts/kubeconfig.sh --env presentation --region us-east-1 --profile minha-conta
set -euo pipefail

ENVIRONMENT=presentation
REGION=us-east-1
PROFILE=""
TUNNEL=false
LOCAL_PORT=16443
KUBECONFIG_PATH="$HOME/.kube/config"
SOURCE_FILE=""
INSTANCE_ID=""

while [ $# -gt 0 ]; do
  case "$1" in
    --env) ENVIRONMENT="$2"; shift 2 ;;
    --region) REGION="$2"; shift 2 ;;
    --profile) PROFILE="$2"; shift 2 ;;
    --tunnel) TUNNEL=true; shift ;;
    --local-port) LOCAL_PORT="$2"; shift 2 ;;
    --kubeconfig) KUBECONFIG_PATH="$2"; shift 2 ;;
    --source-file) SOURCE_FILE="$2"; shift 2 ;;
    --instance-id) INSTANCE_ID="$2"; shift 2 ;;
    *) echo "opção desconhecida: $1" >&2; exit 2 ;;
  esac
done

NAME="fix-$ENVIRONMENT"
AWS_ARGS=(--region "$REGION")
[ -n "$PROFILE" ] && AWS_ARGS+=(--profile "$PROFILE")

if [ -n "$SOURCE_FILE" ]; then
  CONTENT="$(cat "$SOURCE_FILE")"
else
  CONTENT="$(aws ssm get-parameter "${AWS_ARGS[@]}" --name "/fix/$ENVIRONMENT/kubeconfig" --with-decryption --query Parameter.Value --output text)"
fi
if ! grep -q 'apiVersion: *v1' <<<"$CONTENT"; then
  echo "O kubeconfig ainda não foi publicado pelo nó (valor atual: '$CONTENT'). Aguarde o bootstrap terminar (~5 min)." >&2
  exit 1
fi

if [ "$TUNNEL" = true ]; then
  CONTENT="$(sed -E "s#server: *https://[^ ]+:6443#server: https://127.0.0.1:$LOCAL_PORT#" <<<"$CONTENT")"
fi

KUBE_DIR="$(dirname "$KUBECONFIG_PATH")"
mkdir -p "$KUBE_DIR"
CLUSTER_FILE="$KUBE_DIR/$NAME.yaml"
printf '%s\n' "$CONTENT" > "$CLUSTER_FILE"
chmod 600 "$CLUSTER_FILE"

# Mescla: o arquivo novo vem primeiro, então suas entradas (mesmo nome) substituem as antigas.
SOURCES="$CLUSTER_FILE"
if [ -f "$KUBECONFIG_PATH" ]; then
  cp "$KUBECONFIG_PATH" "$KUBECONFIG_PATH.bak"
  SOURCES="$CLUSTER_FILE:$KUBECONFIG_PATH"
fi
MERGED="$(KUBECONFIG="$SOURCES" kubectl config view --flatten)"
printf '%s\n' "$MERGED" > "$KUBECONFIG_PATH"
chmod 600 "$KUBECONFIG_PATH"
kubectl config use-context "$NAME" --kubeconfig "$KUBECONFIG_PATH" >/dev/null

echo "Contexto '$NAME' adicionado a $KUBECONFIG_PATH e selecionado (backup em $KUBECONFIG_PATH.bak)."
if [ "$TUNNEL" = true ]; then
  INSTANCE="$INSTANCE_ID"
  if [ -z "$INSTANCE" ]; then
    INSTANCE="$(aws ec2 describe-instances "${AWS_ARGS[@]}" --filters "Name=tag:Name,Values=$NAME" "Name=instance-state-name,Values=running" --query 'Reservations[0].Instances[0].InstanceId' --output text 2>/dev/null || true)"
    [ -n "$INSTANCE" ] && [ "$INSTANCE" != None ] || INSTANCE="<instance_id: terraform output -raw instance_id>"
  fi
  echo
  echo "Abra o túnel em outro terminal e mantenha-o aberto enquanto usar o kubectl:"
  echo "  aws ssm start-session ${AWS_ARGS[*]} --target $INSTANCE --document-name AWS-StartPortForwardingSession --parameters portNumber=6443,localPortNumber=$LOCAL_PORT"
else
  echo "Acesso direto: o seu IP precisa estar em admin_cidrs no Terraform. Teste com: kubectl get nodes"
fi
