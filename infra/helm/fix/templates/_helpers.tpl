{{/* Tag das imagens: image.tag ou, por padrão, a appVersion do chart (SHA do build que gerou as imagens). */}}
{{- define "fix.imageTag" -}}
{{- .Values.image.tag | default .Chart.AppVersion -}}
{{- end -}}

{{/* Nome base dos recursos: o nome do release (ex.: "fix"). */}}
{{- define "fix.fullname" -}}
{{- .Release.Name | trunc 40 | trimSuffix "-" -}}
{{- end -}}

{{/* Labels comuns; uso: include "fix.labels" (dict "ctx" . "component" "bff") */}}
{{- define "fix.labels" -}}
app.kubernetes.io/name: fix
app.kubernetes.io/instance: {{ .ctx.Release.Name }}
app.kubernetes.io/component: {{ .component }}
app.kubernetes.io/version: {{ include "fix.imageTag" .ctx | quote }}
app.kubernetes.io/managed-by: {{ .ctx.Release.Service }}
helm.sh/chart: {{ printf "%s-%s" .ctx.Chart.Name .ctx.Chart.Version | replace "+" "_" }}
{{- end -}}

{{- define "fix.selectorLabels" -}}
app.kubernetes.io/name: fix
app.kubernetes.io/instance: {{ .ctx.Release.Name }}
app.kubernetes.io/component: {{ .component }}
{{- end -}}

{{/* Imagem de um componente da aplicação: <registry>/<componente>:<tag> */}}
{{- define "fix.image" -}}
{{- printf "%s/%s:%s" .ctx.Values.image.registry .component (include "fix.imageTag" .ctx) -}}
{{- end -}}

{{- define "fix.pullSecrets" -}}
{{- with .Values.image.pullSecrets }}
imagePullSecrets:
{{- toYaml . | nindent 2 }}
{{- end }}
{{- end -}}

{{/* URL pública (CORS do BFF) */}}
{{- define "fix.publicUrl" -}}
{{- if .Values.ingress.host -}}
{{- printf "%s://%s" (ternary "https" "http" .Values.ingress.tls.enabled) .Values.ingress.host -}}
{{- else -}}
*
{{- end -}}
{{- end -}}

{{- define "fix.postgresHost" -}}{{ include "fix.fullname" . }}-postgres{{- end -}}
{{- define "fix.redisHost" -}}{{ include "fix.fullname" . }}-redis{{- end -}}
{{- define "fix.elasticsearchHost" -}}{{ include "fix.fullname" . }}-elasticsearch{{- end -}}
{{- define "fix.coreHost" -}}{{ include "fix.fullname" . }}-core-service{{- end -}}

{{/* Telemetria: ligada explicitamente ou quando a stack de observabilidade roda no cluster. */}}
{{- define "fix.telemetryEnabled" -}}
{{- or .Values.telemetry.enabled .Values.observability.enabled -}}
{{- end -}}

{{- define "fix.collectorHost" -}}{{ include "fix.fullname" . }}-otel-collector{{- end -}}

{{- define "fix.otlpGrpcEndpoint" -}}
{{- .Values.telemetry.otlpGrpcEndpoint | default (ternary (printf "http://%s:4317" (include "fix.collectorHost" .)) "" .Values.observability.enabled) -}}
{{- end -}}

{{- define "fix.otlpHttpEndpoint" -}}
{{- .Values.telemetry.otlpHttpEndpoint | default (ternary (printf "http://%s:4318" (include "fix.collectorHost" .)) "" .Values.observability.enabled) -}}
{{- end -}}
