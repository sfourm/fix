# Fix

Plataforma multitenant de gestão de riscos de commodities (modelo FIX2, ver `example/FIX2_v60.html`):

```
Setup da companhia → Política de riscos (aprovada em ata) → Mandatos (autorizam volume num eixo) → Boletas de hedge → Confirmation
```

```
fix/
├── protos/     # Contratos gRPC (fonte única, consumida pelo backend e pelo BFF)
├── backend/    # Core em .NET 10 (Clean Architecture + DDD + CQS + gRPC)
├── frontend/
│   ├── bff/    # BFF em Node.js + TypeScript + Express (REST para o web, gRPC para o core)
│   └── web/    # Vue 3 + Vite + TypeScript + Pinia + Vue Router
└── example/    # Protótipo HTML do FIX2 (referência de telas e regras)
```

## Rodando tudo localmente

```bash
# 1. Banco + core
cd backend && docker compose up -d && dotnet run --project src/Fix.Presentation   # gRPC :5098

# 2. BFF (outro terminal)
cd frontend/bff && npm install && npm run dev                                     # http://localhost:3000

# 3. Web (outro terminal)
cd frontend/web && npm install && npm run dev                                     # http://localhost:5173
```

Acesse http://localhost:5173, crie uma conta e uma organização (ou entre com `admin@fix.local` / `Admin@12345`).

## Fluxo técnico

```
Web (Vue) --Bearer + X-Organization-Id--> BFF (Node) --gRPC + RequestContext{user_id, organization_id}--> Core (.NET) --> PostgreSQL
```

- O **BFF** autentica o usuário (token de sessão próprio) e envia `context.user_id` e `context.organization_id` em cada request gRPC.
- O **core** confia no `user_id` recebido e **valida as roles consultando a base**: membership, rules diretas do membro,
  rules herdadas dos grupos e a rule de plataforma `super_administrador`.
- O core não deve ficar exposto publicamente, só na rede interna com o BFF.

## Modelo de domínio

| Agregado | O que guarda | Regras principais |
| --- | --- | --- |
| **Organization** | Perfil (razão social, CNPJ, setor, safra ativa), capacidade industrial, orçamento (custo caixa, piso econômico), financeiro, commodities, membros (com **mesa**), grupos | Quem cria vira `founder` e entra no grupo padrão de administradores |
| **Counterparty** | Contrapartes, tipo, limites nocional/MtM, homologação | Só homologadas aceitam boletas; com boletas não podem ser excluídas |
| **Policy** | Política-mãe versionada: limites (§6–§8), eixos por fator de risco, bandas de cobertura por safra, instrumentos permitidos, histórico de versões | `Draft → UnderApproval → Active (ata) → Superseded`. Só rascunho/em aprovação é editável; aprovar substitui a vigente |
| **Mandate** | Autorização de volume/preço/janela num eixo da política; consumido e saldo | Tipo ↔ fator do eixo. Enquadramento automático (vigência, horizonte, janela, piso econômico/gatilho). Dentro + `self_approve` → ativo; senão fila. Fora da política só aprova quem tem `approve_exception` |
| **Order** (boleta) | Futuro, opção ou NDF sobre um mandato ativo | Consome saldo só quando aprovada. Confirmation (middle office): pendente → confirmado / divergente / recusado; atrasado após 2 dias úteis |
| **Role / Rule** | Roles = 22 permissões atômicas; rules = conjuntos atribuídos a membros e grupos | Rules: `super_administrador`, `administrador`, `founder`, `gestor`, `operador`, `middle_office`, `user` |

A alçada de emissão é a role `self_approve`: sem ela, mandatos e boletas vão para a **fila de aprovação**.

### Auditoria / timeline

Todas as entidades têm `created_at`, `updated_at`, `author_created` e `author_updated` como **shadow properties**
(o domínio não conhece esses campos). O `AuditingInterceptor` do EF preenche esses campos e grava cada mudança
(Created/Updated/Deleted, com old/new) na timeline do agregado (`OrganizationTimeline`, `PolicyTimeline`, `MandateTimeline`...,
tabela `timelines` via TPH). Mudanças em entidades filhas (eixo, banda, membro, commodity...) entram na timeline do agregado dono,
com o nome da entidade como prefixo dos campos (`PolicyAxis.Title`).

## Backend

| Camada | Projeto | Responsabilidade |
| --- | --- | --- |
| Presentation | `Fix.Presentation` | Serviços gRPC, server reflection, mapeamento contrato ↔ DTO, tradução de erros para status gRPC |
| Application | `Fix.Application` | `UseCases/<Módulo>/{Commands,Queries,Services,Validators,Mappers,Dtos}`, pipeline (contexto → autorização → validação) |
| Domain | `Fix.Domain` | `AggregateRoots/<Agregado>/{Entities,Enums,ValueObjects,Repositories,Timeline,Statics}`, `Services/MandateCompliance`, `Common` |
| Infrastructure | `Fix.Infrastructure` | EF Core + PostgreSQL, repositórios, UnitOfWork, Identity, interceptor de auditoria/timeline |

Autorização é declarativa no contrato: `[RequireRole(RoleCodes.ApproveMandate)]` ou `[RequireMembership]`.

```bash
cd backend
docker compose up -d                       # PostgreSQL em localhost:5432 (fix/fix)
dotnet run --project src/Fix.Presentation  # gRPC em http://localhost:5098 (HTTP/2); migrations e seed em Development
dotnet test                                # testes de domínio
dotnet ef migrations add <Nome> -p src/Fix.Infrastructure -s src/Fix.Presentation -o Persistence/Migrations
grpcurl -plaintext localhost:5098 list     # contratos via server reflection
```

## Frontend

### BFF (`frontend/bff`)

```
src/
├── cross-cutting/   # Compartilhado: AppError, RequestContext, SessionUser, Page, enums (PascalCase, iguais ao domínio)
├── application/     # Um módulo por entidade: auth, organizations, counterparties, policies, mandates, orders, roles, rules, timeline
│   └── mandates/
│       ├── commands/    # Contratos de entrada que alteram estado (IssueMandateCommand, MandateTermsInput...)
│       ├── queries/     # Contratos de leitura (ListMandatesQuery, PreviewMandateComplianceQuery)
│       ├── responses/   # Contratos de saída para o web
│       ├── dtos/        # Dados trazidos da infraestrutura para a application
│       ├── ports/       # Interface do gateway (implementada na infraestrutura)
│       ├── mappers/     # Dto -> Response
│       └── services/    # Orquestra o caso de uso
├── infrastructure/
│   ├── config/      # Variáveis de ambiente (zod)
│   ├── grpc/        # Contratos (.proto ou reflection), CoreClient, gateways, mappers contrato ↔ Dto (enums COMMODITY_RAW_SUGAR ↔ RawSugar)
│   └── security/    # Token de sessão JWT (HS256) emitido pelo BFF
└── presentation/http/  # Express: middlewares (sessão, tenant, erros), validação (zod) e rotas
```

Erros gRPC viram HTTP: `INVALID_ARGUMENT` 400, `UNAUTHENTICATED` 401, `PERMISSION_DENIED` 403, `NOT_FOUND` 404,
`ALREADY_EXISTS` 409, `FAILED_PRECONDITION` 422 (regra de negócio), `UNAVAILABLE` 503.

| Rota | Descrição |
| --- | --- |
| `POST /api/auth/register`, `/login`, `GET /me` | Conta e sessão |
| `GET, POST /api/organizations` | Organizações do usuário / criar |
| `GET, PUT /api/organization` · `PUT /profile`, `/industrial`, `/budget`, `/financials` | Setup da companhia |
| `POST /api/organization/commodities` · `PUT, DELETE /:id` | Commodities |
| `GET /api/organization/roles` | Roles efetivas (alçadas) do usuário |
| `GET, POST /api/organization/members` · `PATCH /:id/desk` · `DELETE /:id` | Membros (rule + mesa) |
| `GET, POST /api/organization/groups` · `POST /:id/members` | Grupos (com rules) |
| `GET, POST /api/counterparties[?onlyHomologated]` · `GET, PUT, DELETE /:id` · `PATCH /:id/homologation` | Contrapartes |
| `GET, POST /api/policies` · `GET, PUT, DELETE /:id` · `PUT /:id/limits` | Política |
| `POST /api/policies/:id/submit`, `/approve` (ata), `/versions` | Ciclo de aprovação |
| `POST, PUT, DELETE /api/policies/:id/{axes,bands,instruments}[/:childId]` | Eixos, bandas e instrumentos |
| `GET, POST /api/mandates[?policyId&status]` · `POST /preview` · `GET, PUT, DELETE /:id` · `POST /:id/{approve,reject,close}` | Mandatos |
| `GET, POST /api/orders[?mandateId&approval&confirmation]` · `GET, PUT, DELETE /:id` · `POST /:id/{approve,reject}` | Boletas |
| `POST /api/orders/:id/{confirmation,divergence,refusal,resolve}` | Confirmation (middle office) |
| `GET /api/roles`, `/api/rules` | Catálogo de roles e rules |
| `GET /api/timeline?entityType=&entityId=` | Auditoria |

### Web (`frontend/web`)

```
src/
├── domain/          # Tipos por entidade (organization, counterparty, policy, mandate, order, role, rule), permissões e rótulos pt-BR
├── application/     # Stores Pinia (sessão, organização + roles, catálogo de roles/rules, contadores das filas) e provedor da API
├── infrastructure/  # HttpClient (Authorization + X-Organization-Id), APIs REST por entidade, localStorage
└── presentation/    # Router (guards de sessão/tenant/role), layouts, views, componentes e composables
```

Menu na ordem do processo do FIX:

- **Visão geral**: KPIs (política vigente, mandatos ativos, fila, confirmations), checklist de configuração, últimas alterações.
- **1 · Configurar**: Setup da companhia (identificação, orçamento/gatilhos, capacidade, commodities, financeiro), Contrapartes,
  Membros e grupos (rule + mesa), Rules e alçadas, Política de riscos (parâmetros, eixos, bandas, instrumentos, versões, mandatos, histórico).
- **2 · Autorizar**: Mandatos, com prévia de enquadramento ao vivo antes de emitir.
- **3 · Operar**: Boletas de hedge, Boletas em aberto (confirmations pendentes/divergentes/recusados), Fila de aprovação.
- **4 · Acompanhar**: Timeline.

Botões aparecem conforme as roles do usuário, mas quem autoriza de fato é o core.

## Próxima fase (fora do escopo atual)

Estruturas do FIX2 documentadas para implementação futura, seguindo o mesmo padrão de agregados:

| Estrutura | Papel no FIX2 | Ligação com o que existe |
| --- | --- | --- |
| **Contratos comerciais (físico)** | Venda/compra física com cronograma de entregas, preço fixo/a fixar, prêmio e base | Executam mandatos `Commercial` (eixo físico); boletas de fixação passam a consumir o cronograma |
| **Frete / logística** | Contratos de frete por rota e período, tarifa vs. referência | Executam mandatos `Logistics`; limites `logisticsDeadlineMonths` e `freightCeilingPct` da política |
| **Dados de produção** | Estimativa de moagem, ATR, mix e contingência por safra | Definem o disponível para vender/fixar; base do teto absoluto e das bandas de cobertura |
| **Mercado** | Referências (NY11, Londres, câmbio, ESALQ) e curvas | Alimenta MtM, estresse de margem (`marginCashMaxPct`) e visualizações |
| **Visualizações** | Dashboards por tela, contraparte, contrato, câmbio | Leitura sobre boletas, mandatos e contratos filtrada por safra |
