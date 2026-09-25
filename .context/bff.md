# BFF (`frontend/bff`)

Node ≥ 22, **ESM** (`"type": "module"`), TypeScript estrito (`module: nodenext`, `verbatimModuleSyntax`: imports relativos com
`.js` e `import type` para tipos). Express 5, zod, `@grpc/grpc-js`, Elasticsearch, Redis, OpenTelemetry.

## Responsabilidades

- **Autenticação**: emite e valida o token de sessão próprio (JWT HS256, `SESSION_SECRET`, `SESSION_TTL`). O core não valida token.
- **Tenant**: resolve `X-Organization-Id`, confere a membership (roles cacheadas) e monta o `RequestContext`.
- **REST para o web**: rotas `/api/...`, validação de formato com zod, tradução de erros gRPC → HTTP.
- **Domínio próprio**: dashboards, widgets, filtros salvos e pesquisa (não existem no core).

## Estrutura

```
src/
├── cross-cutting/     # AppError, RequestContext, SessionUser, Page, enums (PascalCase, iguais ao domínio do core)
├── domain/            # entidades e regras do BFF: dashboards (widgets, layout, cores, consulta), filtros salvos,
│                      # visibilidade, SharedResource (quem vê/edita), DomainError e interfaces de repositório
├── application/       # um módulo por entidade
│   └── <módulo>/
│       ├── commands/  # contratos de entrada que alteram estado
│       ├── queries/   # contratos de leitura
│       ├── responses/ # contratos de saída para o web
│       ├── dtos/      # dados trazidos da infraestrutura
│       ├── ports/     # interfaces de gateway (implementadas na infraestrutura)
│       ├── mappers/   # Dto → Response
│       └── services/  # orquestram o caso de uso
├── infrastructure/
│   ├── config/        # env.ts (zod) — todas as variáveis com default de dev
│   ├── grpc/          # CoreClient, carregamento de contratos, gateways, mappers contrato ↔ Dto, grpc-errors
│   ├── elasticsearch/ # repositórios de dashboards e filtros (índices fix-dashboards, fix-saved-filters)
│   ├── cache/         # RedisCache (falha de Redis = "sem cache", nunca erro)
│   └── security/      # token de sessão
├── presentation/http/ # app.ts, middlewares (sessão, tenant, erros), validation.ts / dashboard.validation.ts, routes/, telemetry.middleware.ts
├── instrumentation.ts # OpenTelemetry, carregado com --import antes da aplicação
├── telemetry.ts       # NodeSDK (traces + métricas OTLP HTTP)
└── main.ts            # composição: cria clientes, repositórios e services e injeta manualmente
```

## Fluxo

`rota Express → schema zod → service (application) → port → gateway gRPC → core`
(ou, para dashboards/filtros: `service → domínio do BFF → repositório Elasticsearch`).

## Regras e padrões

- **Ids**: valide com `z.guid()` (não `z.uuid()`): os ids determinísticos do core não seguem RFC-4122.
- **Erros gRPC → HTTP**: `INVALID_ARGUMENT` 400, `UNAUTHENTICATED` 401, `PERMISSION_DENIED` 403, `NOT_FOUND` 404,
  `ALREADY_EXISTS` 409, `FAILED_PRECONDITION` 422 (regra de negócio, mensagem pt-BR do core), `UNAVAILABLE` 503.
- **Escritas** que mudam dados exibidos em dashboards/pesquisa usam `invalidateOnWrite` nas rotas (`app.ts`), limpando o cache da organização.
- Operações sem retorno respondem `204`.

## Dashboards, filtros e pesquisa (domínio do BFF)

- **Visibilidade**: `Private` (só o dono) ou `Public` (toda a organização vê). Editam o dono e, se público, quem tem
  `edit_organization`; só o dono muda a visibilidade. Qualquer membro pode **duplicar** um dashboard público como cópia privada.
- **Widget**: tipo (`Kpi`, `Column`, `Bar`, `Line`, `Donut`, `Table`), largura 2–12 colunas, altura 120–720 px, cores `#RRGGBB`
  (uma cor ou uma por categoria) e consulta: conjunto (orders, mandates, counterparties, policies), medida
  (count/sum/avg/min/max), dimensão, ordem, limite (excedente vira "Outros"), filtro salvo e condições próprias.
- **Dashboard público só usa filtros públicos**; filtro usado por dashboard não pode ser excluído nem ficar privado se o dashboard for público.
- **Templates**: `blank` ou `fix-overview`.
- **Pesquisa**: igualdades suportadas pelas listagens do core (ex.: `approval`, `status`, `mandateId`) vão como parâmetro gRPC;
  as demais condições (contém, entre, maior que...) são aplicadas no BFF. Até 5.000 linhas por conjunto.

## Cache (Redis)

- Chaves (com prefixo `fix:` no Redis): `viz:{org}:rows:{user}:{fonte}:{filtros}` (linhas lidas do core para pesquisa e widgets) e
  `viz:{org}:roles:{user}` (roles do usuário na organização), TTL `VIZ_CACHE_TTL_SECONDS` (30 s).
- Qualquer escrita da organização via BFF invalida `viz:{org}:*`; mudanças por outros canais expiram pelo TTL.

## Telemetria

- `npm run dev` = `node --watch --env-file-if-exists=.env --import tsx --import ./src/instrumentation.ts src/main.ts`.
  **Não usar `tsx watch`**: impede o patch das instrumentações.
- Pacotes CommonJS importados de ESM (ex.: `@grpc/grpc-js`) não passam pelo hook das instrumentações; o `instrumentation.ts`
  os pré-carrega via `require` com o SDK ativo. Sem isso o trace não continua no core.
- `telemetry.middleware.ts` nomeia o span do request `MÉTODO /rota/:param`, registra `http_requests_total` e
  `http_request_duration_ms` e devolve `x-trace-id`/`x-correlation-id`.
- `RedisCache` emite spans `redis GET|SET|INVALIDATE <keyspace>` e `cache_lookups_total{keyspace,result}`.

## Variáveis de ambiente principais

`PORT` (3000), `CORE_GRPC_URL` (localhost:5098), `CORE_GRPC_TLS`, `CORE_GRPC_DEADLINE_MS`, `GRPC_CONTRACTS` (files|reflection),
`PROTOS_DIR`, `SESSION_SECRET` (obrigatório em produção), `SESSION_TTL`, `CORS_ORIGIN`, `ELASTICSEARCH_URL`,
`ELASTICSEARCH_INDEX_PREFIX`, `REDIS_URL`, `VIZ_CACHE_TTL_SECONDS`, `OTEL_ENABLED`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_SERVICE_NAME`.

## Comandos

```bash
cd frontend/bff
docker compose up -d   # Elasticsearch :9200 e Redis :6379
npm install
npm run dev            # http://localhost:3000
npm test               # node:test (domínio e motor de pesquisa) em test/
npm run typecheck
npm run build && npm start
```
