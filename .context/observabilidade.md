# Observabilidade (`observability/`)

## Componentes

| Componente | Função | Acesso |
| --- | --- | --- |
| OpenTelemetry Collector | Recebe OTLP de todos os serviços; exporta traces ao Jaeger e métricas ao Prometheus; gera métricas a partir dos spans | `4317` (gRPC), `4318` (HTTP), `8888` (métricas do coletor), `8889` (métricas das apps) |
| Jaeger | Armazena e mostra traces (em memória: reiniciar apaga) | http://localhost:16686 |
| Prometheus | Séries temporais e exemplars | http://localhost:9090 |
| Grafana | Dashboards (editáveis pela interface) | http://localhost:3001 (`admin`/`admin`) |

```bash
cd observability && docker compose up -d
```

## O que cada serviço emite

| Serviço | Nome (`service.name` / label `job`) | Traces | Métricas |
| --- | --- | --- | --- |
| core-service | `fix-backend` | gRPC server (ASP.NET Core), HttpClient, PostgreSQL (Npgsql) com spans `SELECT orders`, `INSERT mandates`... | `http_server_request_duration_seconds`, `db_client_operation_duration_seconds`, `db_client_connection_count`/`_max`, Kestrel |
| BFF | `fix-bff` | Request HTTP (`MÉTODO /rota/:param`), middlewares Express, cliente gRPC (`grpc.fix.v1.<Serviço>/<Método>`), Redis, Elasticsearch | `http_requests_total`, `http_request_duration_ms`, `cache_lookups_total`, event loop e GC do Node |
| web | — | Não emite (o trace começa no BFF) | — |

Um request vira **um trace único**: `BFF (HTTP) → grpc client → core (gRPC server) → consultas SQL`, com Redis/Elasticsearch
como filhos do request no BFF. Toda resposta do BFF traz `x-trace-id` para achar o trace.

## Pipeline de métricas

- O conector `span_metrics` do coletor gera, para **cada span**, `traces_span_metrics_calls_total` e
  `traces_span_metrics_duration_milliseconds_*` com dimensões: `http.route`, `http.request.method`, `http.response.status_code`,
  `rpc.service`, `rpc.method`, `rpc.grpc.status_code`, `grpc.status_code`, `db.system.name`, `db.operation.name`,
  `db.collection.name`. Assim o mesmo trace alimenta métricas de todas as camadas.
- O exporter Prometheus do coletor **não usa namespace** (métricas sem prefixo) e expõe OpenMetrics (necessário para exemplars).
- `prometheus.yml` usa `honor_labels: true` no scrape do coletor: o label `job` é o serviço (`fix-bff`, `fix-backend`).
- **Exemplars**: Prometheus roda com `--enable-feature=exemplar-storage`; o datasource Prometheus liga `trace_id` ao Jaeger
  (`exemplarTraceIdDestinations`). Pontos nos gráficos de latência abrem o trace.

## Grafana

Datasources (`grafana/provisioning/datasources/datasources.yaml`, uids fixos `prometheus` e `jaeger`) são provisionados por
arquivo; o provisionamento apaga e recria os dois a cada start (`deleteDatasources`), o que corrige volumes antigos com uids gerados.

Dashboards (`grafana/dashboards/*.json`, pasta FIX) **não** são provisionados por arquivo: desde o Grafana 12 a interface trata
dashboard provisionado como "gerenciado" e, ao salvar, só oferece baixar o JSON (mesmo com `allowUiUpdates`). Eles entram pela
API com `grafana/seed-dashboards.sh` (serviço `grafana-seed` no docker-compose; Job `grafana-dashboards-seed`, hook pós-deploy,
no chart), que só cria o que não existe. Assim ficam editáveis e salvos no banco do Grafana; o JSON do repositório é a semente.

### Fix Platform - Observabilidade & Telemetria (`fix-telemetry-overview`)
Visão geral: serviços ativos, taxa de requests, latência p50/p95/p99 por serviço, ingestão e exportação do coletor.

### Fix Platform - Requests, gRPC e banco (`fix-requests-traces`)
Filtros: rota do BFF, serviço gRPC, tabela, Trace ID.

| Seção | Conteúdo |
| --- | --- |
| Visão geral do período | Requests, % 5xx, p95 do BFF, chamadas gRPC, % falhas técnicas gRPC, consultas SQL, **SQL por chamada gRPC**, hit do cache |
| BFF · requests HTTP | Throughput e latência por rota, classes de status, tabela por operação com link para os traces no Jaeger |
| gRPC · BFF → core | Chamadas e latência por método, status (OK × regra de negócio × falha técnica), p95 visto pelo BFF × dentro do core, tabela por método |
| PostgreSQL | Consultas por operação/tabela, latência, consultas por chamada gRPC (detecta N+1), pool do Npgsql, consultas que mais consomem tempo |
| BFF · Redis e Elasticsearch | Hit ratio por keyspace, leituras por resultado, latência dos comandos |
| Traces | Cada ponto é um request com trace (exemplar): hover com operação/status/duração e clique para o Jaeger; mapa de serviços; trace por ID (linha recolhida) |

Classificação de status gRPC usada nos painéis:
- **Regra de negócio/validação**: 3 INVALID_ARGUMENT, 5 NOT_FOUND, 6 ALREADY_EXISTS, 7 PERMISSION_DENIED, 9 FAILED_PRECONDITION, 16 UNAUTHENTICATED.
- **Falha técnica**: 1, 2 UNKNOWN, 4 DEADLINE_EXCEEDED, 8, 10, 11, 12, 13 INTERNAL, 14 UNAVAILABLE, 15.

Limitações conhecidas: exemplars aparecem como pontos em gráficos (o Grafana não os lista como linhas de tabela), e a busca
de traces do Jaeger no Grafana nomeia o trace pelo primeiro span recebido; por isso o dashboard usa exemplars e links para a UI do Jaeger.

## Como instrumentar algo novo

- **core**: `ActivitySource`/`Meter` com o nome do serviço (`AddSource(serviceName)` / `AddMeter(serviceName)` já registrados).
  Novas bibliotecas: adicione a instrumentação em `TelemetryExtensions`.
- **BFF**: `trace.getTracer(...)` / `metrics.getMeter('fix-bff')` de `@opentelemetry/api`. Nova dependência CommonJS que precise
  de auto-instrumentação: inclua em `PRELOAD_COMMONJS` no `instrumentation.ts`. Evite labels de alta cardinalidade (ids): use a
  rota canônica ou o "keyspace", nunca a chave/URL completa.
- **Dashboards**: edite e salve direto no Grafana. Para levar a edição ao repositório (semente de instalações novas), exporte
  o JSON para `observability/grafana/dashboards/`; num Grafana existente, a semente só recria um dashboard apagado. Painéis com razão devem tratar numerador vazio (`or vector(0)`) para não mostrar "sem dados" quando não há erros.

## Configuração nas aplicações

- core: `appsettings.json` → `OpenTelemetry:{Enabled, ServiceName, OtlpEndpoint}` (padrão `http://localhost:4317`) ou
  `OTEL_EXPORTER_OTLP_ENDPOINT`.
- BFF: `OTEL_ENABLED`, `OTEL_EXPORTER_OTLP_ENDPOINT` (padrão `http://localhost:4318`), `OTEL_SERVICE_NAME`.
