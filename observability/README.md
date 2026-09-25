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
  - O dashboard **Fix Platform - Observabilidade & Telemetria** já vem provisionado na pasta `FIX`.
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
  - Consultas SQL do PostgreSQL via EF Core (`Npgsql.OpenTelemetry`)

### Frontend BFF (Node.js)
- Envia traces e métricas via **OTLP HTTP** para `http://localhost:4318`.
- Variáveis de ambiente configuráveis:
  - `OTEL_EXPORTER_OTLP_ENDPOINT`: URL do coletor OTLP (padrão: `http://localhost:4318`).
  - `OTEL_SERVICE_NAME`: Nome do serviço (padrão: `fix-bff`).
  - `OTEL_ENABLED`: Ativa/desativa telemetria (padrão: `true`).
- Coleta automática:
  - Requisições HTTP do Express e rotas REST
  - Chamadas gRPC ao Backend com injeção automática de contexto W3C `traceparent`
  - Comandos no Redis e chamadas HTTP

### Frontend Web (Vue.js 3)
- As requisições HTTP do Web injetam cabeçalhos de contexto W3C `traceparent`.
- O BFF extrai esse contexto e dá continuidade ao trace até o Backend e o banco PostgreSQL.
