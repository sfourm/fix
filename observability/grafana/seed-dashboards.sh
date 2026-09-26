#!/bin/sh
# Semeia os dashboards do FIX no Grafana pela API (só sh + curl).
#
# Por que não provisionar por arquivo: desde o Grafana 12 a interface trata dashboard provisionado por arquivo como
# "gerenciado" (grafana.app/managedBy) e só oferece baixar o JSON ao salvar, mesmo com allowUiUpdates. Semeados pela
# API, viram dashboards comuns: editáveis e salvos no banco do Grafana (volume persistente).
#
# Regra: importa um JSON do repositório só se o dashboard não existe ou se ainda está preso ao provisionamento
# antigo. Dashboard já existente e comum nunca é sobrescrito (edições feitas na interface ficam).
# Para levar uma edição da interface para o repositório: exporte o JSON e salve em observability/grafana/dashboards.
#
# Variáveis: GRAFANA_URL, GRAFANA_USER, GRAFANA_PASSWORD, DASHBOARDS_DIR, FOLDER_TITLE.
set -eu

GRAFANA_URL="${GRAFANA_URL:-http://grafana:3000}"
AUTH="${GRAFANA_USER:-admin}:${GRAFANA_PASSWORD:?defina GRAFANA_PASSWORD}"
DIR="${DASHBOARDS_DIR:-/dashboards}"
FOLDER_TITLE="${FOLDER_TITLE:-FIX}"
TMP="$(mktemp -d)"

api() { curl -sS -u "$AUTH" -H 'Content-Type: application/json' "$@"; }
code() { curl -s -o /dev/null -w '%{http_code}' -u "$AUTH" "$@"; }

echo "Aguardando o Grafana em $GRAFANA_URL..."
tries=0
until [ "$(code "$GRAFANA_URL/api/health")" = 200 ]; do
  tries=$((tries + 1))
  [ "$tries" -gt 90 ] && { echo "O Grafana não respondeu em 3 minutos." >&2; exit 1; }
  sleep 2
done

# Pasta pelo título (a antiga, criada pelo provisionamento, é reaproveitada).
folder_uid() {
  api "$GRAFANA_URL/api/folders" | tr '{' '\n' | grep "\"title\":\"$FOLDER_TITLE\"" | sed -n 's/.*"uid":"\([^"]*\)".*/\1/p' | head -n 1
}
FOLDER="$(folder_uid)"
if [ -z "$FOLDER" ]; then
  api -X POST "$GRAFANA_URL/api/folders" -d "{\"title\":\"$FOLDER_TITLE\"}" > /dev/null
  FOLDER="$(folder_uid)"
fi

# Precisa semear: não existe, ou ainda marcado como provisionado por arquivo.
needs_seed() {
  [ "$(code "$GRAFANA_URL/api/dashboards/uid/$1")" = 404 ] && return 0
  api "$GRAFANA_URL/apis/dashboard.grafana.app/v1beta1/namespaces/default/dashboards/$1" | grep -q '"grafana.app/managedBy"' && return 0
  return 1
}

# Duas passadas: ao remover o provedor de arquivos, o próprio Grafana apaga os dashboards órfãos logo após subir;
# a segunda passada recria o que ele tiver apagado depois da primeira.
seed() {
  pending=0
  for file in "$DIR"/*.json; do
    uid="$(sed -n 's/^  "uid": *"\([^"]*\)".*/\1/p' "$file" | head -n 1)"
    [ -z "$uid" ] && { echo "sem uid, ignorado: $file"; continue; }
    if ! needs_seed "$uid"; then
      echo "mantido (já existe): $uid"
      continue
    fi
    { printf '{"folderUid":"%s","overwrite":true,"message":"semente do repositório","dashboard":' "$FOLDER"; cat "$file"; printf '}'; } > "$TMP/body.json"
    status="$(curl -s -o "$TMP/resp" -w '%{http_code}' -u "$AUTH" -H 'Content-Type: application/json' -X POST "$GRAFANA_URL/api/dashboards/db" --data-binary "@$TMP/body.json")"
    if [ "$status" = 200 ]; then
      echo "semeado: $uid"
    else
      echo "ainda não deu ($status): $uid · $(cat "$TMP/resp")"
      pending=1
    fi
  done
  return "$pending"
}

seed || true
sleep 20
seed
echo "Dashboards prontos."
