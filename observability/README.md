# Camada de Observabilidade & Telemetria - Fix Platform

Esta camada provê observabilidade completa ponta a ponta (Traces Distribuídos, Métricas e Dashboards) para todas as camadas da plataforma Fix:
- **Backend (.NET 10 gRPC + EF Core / PostgreSQL)**
- **BFF (Node.js + Express + gRPC Client + Redis + Elasticsearch)**
- **Frontend Web (Vue.js 3 + Vite)**

---

## 🛠 Componentes da Pilha

| Componente | Função | Porta / Acesso |
|---|---|---|
| **OpenTelemetry Collector** | Ingestão unificada de traces e métricas (OTLP gRPC e HTTP) | `localhost:4317` (gRPC), `localhost:4318` (HTTP), `8888` (Métricas OTel), `8889` (Métricas App) |
| **Jaeger** | Armazenamento e interface de análise de Tracing Distribuído | [http://localhost:16686](http://localhost:16686) |
| **Prometheus** | Scraping e banco de séries temporais de métricas | [http://localhost:9090](http://localhost:9090) |
| **Grafana** | Dashboards unificados e correlação de métricas com traces | [http://localhost:3001](http://localhost:3001) (`admin` / `admin`) |

---

## 🚀 Como Iniciar

### 1. Subir os Contêineres de Observabilidade
```bash
cd observability
docker compose up -d
```

Verifique se todos os serviços estão saudáveis:
```bash
docker compose ps
```

### 2. Acessar as Interfaces
- **Grafana**: [http://localhost:3001](http://localhost:3001) (usuário: `admin`, senha: `admin`)
  - O datasource **Prometheus** e **Jaeger** já vêm pré-configurados!
  - Dashboards provisionados na pasta `FIX`:
    - **Fix Platform - Observabilidade & Telemetria**: visão geral dos serviços e do coletor.
    - **Fix Platform - Requests, gRPC e banco**: requests do BFF e o caminho de cada uma até o banco (detalhes abaixo).
- **Jaeger UI**: [http://localhost:16686](http://localhost:16686)
- **Prometheus UI**: [http://localhost:9090](http://localhost:9090)

---

## 🔗 Integração com as Aplicações

### Backend (.NET 10)
- Envia traces e métricas via **OTLP gRPC** para `http://localhost:4317`.
- Variáveis de ambiente configuráveis:
  - `OTEL_EXPORTER_OTLP_ENDPOINT`: URL do coletor OTLP (padrão: `http://localhost:4317`).
  - `OTEL_SERVICE_NAME`: Nome do serviço no Jaeger/Prometheus (padrão: `fix-backend`).
- Coleta automática:
  - Requisições gRPC / ASP.NET Core (`OpenTelemetry.Instrumentation.AspNetCore`)
  - Chamadas HTTP de saída (`OpenTelemetry.Instrumentation.Http`)
  - Consultas SQL do PostgreSQL via EF Core (`Npgsql.OpenTelemetry`): os spans recebem nome `OPERAÇÃO tabela` (`SELECT orders`,
    `INSERT mandates`) e os atributos `db.operation.name`/`db.collection.name` (`Fix.Infrastructure/Persistence/Telemetry/SqlCommandTelemetry.cs`)
  - Métricas do pool de conexões e das operações do Npgsql (`db_client_connection_count`, `db_client_operation_duration_seconds`)

### Frontend BFF (Node.js)
- Envia traces e métricas via **OTLP HTTP** para `http://localhost:4318`.
- Variáveis de ambiente configuráveis:
  - `OTEL_EXPORTER_OTLP_ENDPOINT`: URL do coletor OTLP (padrão: `http://localhost:4318`).
  - `OTEL_SERVICE_NAME`: Nome do serviço (padrão: `fix-bff`).
  - `OTEL_ENABLED`: Ativa/desativa telemetria (padrão: `true`).
- A instrumentação entra por `--import ./src/instrumentation.ts` (nos scripts `dev` e `start`), antes da aplicação. O BFF é ESM e
  pacotes CommonJS importados de ESM (como `@grpc/grpc-js`) não passam pelo hook das instrumentações; por isso o
  `instrumentation.ts` os pré-carrega via `require` com o SDK já ativo. Sem isso o trace do BFF não continua no core.
- Coleta automática:
  - Requisições HTTP do Express: o span do request recebe o nome `MÉTODO /rota/:param` (`telemetry.middleware.ts`)
  - Chamadas gRPC ao Backend com propagação W3C `traceparent` (um trace único BFF → core → PostgreSQL)
  - Elasticsearch e comandos do Redis
- Cache de visualização (`RedisCache`): spans `redis GET|SET|INVALIDATE <keyspace>` e a métrica `cache_lookups_total{keyspace, result=hit|miss|error}`.
- Toda resposta traz o header `x-trace-id` (e `x-correlation-id`) para achar o trace de um request específico.

### Frontend Web (Vue.js 3)
- O web não gera `traceparent`: sem um SDK que exporte os spans do browser, o pai informado nunca chega ao Jaeger e o trace
  fica sem raiz. O trace começa no BFF; para correlacionar, use o header `x-trace-id` da resposta.

---

## 📈 Dashboard "Requests, gRPC e banco"

Métricas RED (chamadas, erros, latência) de **cada span** são geradas no coletor pelo conector `span_metrics`
(`traces_span_metrics_calls_total`, `traces_span_metrics_duration_milliseconds_*`), com dimensões de rota HTTP, método/status
gRPC e operação/tabela SQL. Assim o mesmo trace alimenta métricas de todas as camadas.

| Seção | O que mostra |
|---|---|
| Visão geral do período | Requests, % de 5xx, p95 do BFF, chamadas gRPC, % de falhas técnicas gRPC, consultas SQL, SQL por chamada gRPC, hit do cache |
| BFF · requests HTTP | Throughput e latência por rota, classes de status e tabela por operação (link para os traces no Jaeger) |
| gRPC · BFF → core | Chamadas e latência por método, status (OK × regra de negócio × falha técnica), p95 visto pelo BFF × dentro do core |
| PostgreSQL | Consultas por operação/tabela, latência, **consultas por chamada gRPC** (detecta N+1), pool do Npgsql, consultas que mais consomem tempo |
| Redis e Elasticsearch | Hit ratio por keyspace, leituras por resultado, latência dos comandos |
| Traces | Cada ponto é um request com trace (exemplar): o hover mostra operação/status/duração e o clique abre o Jaeger; mapa de serviços; trace por ID (linha recolhida) |

- Filtros: rota do BFF, serviço gRPC, tabela e Trace ID.
- **Exemplars**: o Prometheus roda com `--enable-feature=exemplar-storage` e o datasource liga `trace_id` ao Jaeger; os pontos nos
  gráficos de latência abrem o trace correspondente.
- O dashboard é gerado a partir de queries consistentes; ao editar pelo Grafana, exporte o JSON para
  `grafana/dashboards/fix-requests-traces.json` (o provisionamento relê a pasta a cada 10 s).
- O `prometheus.yml` usa `honor_labels: true` no scrape do coletor: o label `job` de cada série é o serviço (`fix-bff`, `fix-backend`).
