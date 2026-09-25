# Arquitetura do sistema

## Aplicações

```
fix/
├── protos/                  # contratos gRPC (fonte única)
├── backend/
│   └── core-service/        # .NET 10 — regras de negócio, autorização, persistência (gRPC)
├── frontend/
│   ├── bff/                 # Node.js + TypeScript + Express — autenticação, REST para o web, dashboards/filtros/pesquisa
│   └── web/                 # Vue 3 + Vite + Pinia — interface
├── observability/           # OTel Collector, Jaeger, Prometheus, Grafana
├── infra/                   # Terraform (EC2 + k3s na AWS), chart Helm e scripts do kubectl — ver infraestrutura.md
├── .github/workflows/       # CI (PR) e Deploy (main → GHCR → k3s)
└── example/                 # protótipo HTML do FIX2
```

`backend/` agrupa as APIs por serviço. Novas APIs entram como pastas irmãs (`backend/<nome>-service/`), cada uma com solução,
`Directory.Build.props`, `dotnet-tools.json` e `docker-compose.yml` próprios (com `name:` fixo no compose e volumes nomeados).

## Fluxo de um request

```
Web (Vue) ──REST: Bearer + X-Organization-Id──▶ BFF (Node) ──gRPC: RequestContext{user_id, organization_id}──▶ core-service (.NET) ──▶ PostgreSQL
                                                 ├── Elasticsearch (dashboards e filtros salvos)
                                                 └── Redis (cache de visualização)

Todos ──OTLP──▶ OTel Collector ──▶ Jaeger (traces) / Prometheus (métricas) ──▶ Grafana
```

1. O web envia o token de sessão (emitido pelo BFF) e a organização atual.
2. O BFF autentica, resolve o tenant (confere a membership com cache), valida o formato (zod) e chama o core via gRPC com o
   `RequestContext`.
3. O core valida a entrada, **autoriza consultando a base** (roles efetivas + organograma), executa a regra de negócio,
   persiste e audita.
4. Erros de domínio voltam como status gRPC e o BFF traduz para HTTP (ver [core-service.md](core-service.md) e [bff.md](bff.md)).

## Responsabilidades

| Aplicação | É dona de | Não faz |
| --- | --- | --- |
| **web** | Telas, navegação, estado de UI, rótulos pt-BR | Regra de negócio; decidir permissão (só esconde ações) |
| **BFF** | Sessão (JWT HS256 própria), tenant do request, REST, validação de formato, **dashboards, filtros salvos e pesquisa**, cache | Regras do domínio FIX |
| **core-service** | Regras de negócio, **autorização por roles na base**, persistência, auditoria/timeline | Validar token (é gRPC interno e confia no `user_id` do contexto); conhecer dashboards/filtros |

## Decisões de arquitetura

- **Core interno**: não fica exposto publicamente, só na rede com o BFF. Por isso não valida token; a autenticação é do BFF.
- **Autorização no core**: roles são sempre checadas no core consultando a base, mesmo que o BFF também confira membership.
- **BFF com domínio próprio**: dashboards, widgets, filtros salvos e pesquisa são preferências de visualização, não regras do
  FIX. Vivem só no BFF (`frontend/bff/src/domain`, persistência no Elasticsearch). Não criar tabelas, contratos ou regras disso
  no core.
- **Contrato único**: `protos/` é a fonte da verdade da comunicação BFF ↔ core.
- **Clean Architecture nos dois lados**: core (Domain / Application / Infrastructure / Presentation) e BFF
  (domain / application / infrastructure / presentation + cross-cutting). O web segue a mesma separação.
- **CQS sem mediator**: commands/queries são records; services com métodos nomeados; autorização por proxy dinâmico.
- **Auditoria transparente**: o domínio não conhece campos de auditoria (shadow properties preenchidas por interceptor).
- **Observabilidade ponta a ponta**: um trace atravessa BFF → gRPC → core → PostgreSQL; métricas por camada derivadas dos spans.

## Portas locais

| Serviço | Porta |
| --- | --- |
| Web (Vite, proxy `/api` → BFF) | 5173 |
| BFF | 3000 |
| core-service (gRPC, HTTP/2) | 5098 |
| PostgreSQL | 5432 |
| Elasticsearch | 9200 |
| Redis | 6379 |
| Grafana | 3001 |
| Jaeger UI | 16686 |
| Prometheus | 9090 |
| OTLP (gRPC / HTTP) | 4317 / 4318 |
