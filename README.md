# Fix

Plataforma multitenant de gestão de riscos de commodities (modelo FIX2, ver `example/FIX2_v60.html`):

```
Setup da companhia → Política de riscos (aprovada em ata) → Mandatos (autorizam volume num eixo) → Boletas de hedge → Confirmation
```

```
fix/
├── protos/         # Contratos gRPC (fonte única, consumida pelo backend e pelo BFF)
├── backend/        # Core em .NET 10 (Clean Architecture + DDD + CQS + gRPC)
├── frontend/
│   ├── bff/        # BFF em Node.js + TypeScript + Express (REST para o web, gRPC para o core)
│   └── web/        # Vue 3 + Vite + TypeScript + Pinia + Vue Router
├── observability/  # Stack de telemetria e observabilidade (OTel Collector + Jaeger + Prometheus + Grafana)
└── example/        # Protótipo HTML do FIX2 (referência de telas e regras)
```

## Rodando tudo localmente

```bash
# 0. Observabilidade (OTel Collector, Jaeger, Prometheus, Grafana)
cd observability && docker compose up -d                                           # Grafana :3001, Jaeger :16686

# 1. Banco + core
cd backend && docker compose up -d && dotnet run --project src/Fix.Presentation   # gRPC :5098

# 2. BFF (outro terminal): Elasticsearch + Redis e o BFF
cd frontend/bff && docker compose up -d && npm install && npm run dev             # http://localhost:3000

# 3. Web (outro terminal)
cd frontend/web && npm install && npm run dev                                     # http://localhost:5173
```

Acesse http://localhost:5173, crie uma conta e uma organização. A equipe interna entra com o super administrador `admin@fix.local` / `Admin@12345`
(criado no startup, configurável em `Seed:SuperAdministrator`).

## Fluxo técnico

```
Web (Vue) --Bearer + X-Organization-Id--> BFF (Node) --gRPC + RequestContext{user_id, organization_id}--> Core (.NET) --> PostgreSQL
                                              ├── Elasticsearch (dashboards e filtros salvos: domínio do BFF)
                                              └── Redis (cache de visualização)
```

- O **BFF** autentica o usuário (token de sessão próprio) e envia `context.user_id` e `context.organization_id` em cada request gRPC.
- O **core** confia no `user_id` recebido e **valida as roles consultando a base**: membership, rules diretas do membro,
  rules herdadas dos grupos e, para a equipe interna FIX em suporte, as roles de suporte (sem decisões).
- O core não deve ficar exposto publicamente, só na rede interna com o BFF.
- **Dashboards, filtros salvos e pesquisa são exclusivos do BFF** (preferências de visualização, não regras de negócio):
  o core não conhece essas entidades; o BFF lê os dados pelas listagens gRPC existentes, com o contexto do usuário.

## Modelo de domínio

| Agregado | O que guarda | Regras principais |
| --- | --- | --- |
| **Organization** | Perfil (razão social, CNPJ, setor, safra ativa), capacidade industrial, orçamento (custo caixa, piso econômico), financeiro, commodities, membros (com **mesa**), grupos | Sempre tem um **owner** (quem cria; transferível); os demais membros são **user**. Grupo raiz "Direção" |
| **Counterparty** | Contrapartes, tipo, limites nocional/MtM, homologação | Só homologadas aceitam boletas; com boletas não podem ser excluídas |
| **Policy** | Política-mãe versionada: limites (§6–§8), eixos por fator de risco, bandas de cobertura por safra, instrumentos permitidos, histórico de versões | `Draft → UnderApproval → Active (ata) → Superseded`. Só rascunho/em aprovação é editável; aprovar substitui a vigente |
| **Mandate** | Autorização de volume/preço/janela num eixo da política; consumido e saldo | Tipo ↔ fator do eixo. Enquadramento automático (vigência, horizonte, janela, piso econômico/gatilho). Dentro + `self_approve` → ativo; senão fila. Fora da política só aprova quem tem `approve_exception` |
| **Order** (boleta) | Futuro, opção ou NDF sobre um mandato ativo | Consome saldo só quando aprovada. Confirmation (middle office): pendente → confirmado / divergente / recusado; atrasado após 2 dias úteis |
| **Role / Rule** | Roles = 22 permissões atômicas; rules = conjuntos atribuídos a membros e grupos | Fixas: `owner`, `user` (clientes) e `super_administrador`, `administrador` (internas). Alçadas personalizadas por organização |

A alçada de emissão é a role `self_approve`: sem ela, mandatos e boletas vão para a **fila de aprovação**.

### Multitenant, equipe interna e alçadas

- **Organização FIX (interna)**: criada no startup com o **super administrador**, que adiciona e remove os **administradores**.
  Esses papéis são internos e nunca aparecem nem são atribuíveis nas organizações clientes.
- **Suporte**: super administrador e administradores veem todas as organizações ("Suporte FIX" na escolha de organização) e
  **visualizam e editam** (setup, membros, alçadas, política...), mas **não decidem**: as roles de decisão (`approve_policy`,
  `approve_mandate`, `approve_exception`, `approve_order`, `manage_confirmation`, `self_approve`) ficam de fora.
- **Owner e user**: toda organização cliente tem exatamente um owner (todas as roles), que não pode ser removido; a propriedade é
  transferível pelo owner ou pelo suporte (o antigo owner vira user). Os demais membros são user (leitura).
- **Alçadas personalizadas**: em Usuários › Rules e alçadas (`/access`) a organização cria, edita e exclui as próprias alçadas
  a partir das roles disponíveis e as atribui a membros e grupos; somam permissões à base (owner/user). Gestor, Operador e
  Middle office entram como **modelos editáveis** copiados para cada organização nova.
- A migration `InternalStaffAndCustomRules` converteu os dados existentes: modelos copiados por organização (atribuições remapeadas),
  `founder` → `owner`, atribuições internas removidas das organizações clientes e grupo "Administradores" → "Direção".

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
| Application | `Fix.Application` | `UseCases/<Módulo>/{Commands,Queries,Services,Validators,Mappers,Dtos}`; interfaces dos services em `Common/Interfaces/UseCases` |
| Domain | `Fix.Domain` | `AggregateRoots/<Agregado>/{Entities,Enums,ValueObjects,Repositories,Timeline,Statics}`, `Services/MandateCompliance`, `Common` |
| Infrastructure | `Fix.Infrastructure` | EF Core + PostgreSQL, repositórios, UnitOfWork, Identity, interceptor de auditoria/timeline |

Fluxo de um caso de uso:

```
gRPC service (Presentation)
  → IValidationFactory.ValidateAsync(command)     # validators do FluentValidation, agnóstico ao tipo (ICommand/IQuery : IUseCase)
  → IPolicyService.CreatePolicyAsync(command)     # service com métodos nomeados (sem ICommandHandler/IQueryHandler)
      ↳ UseCaseGuardProxy → UseCaseGuard          # contexto (user/organization) + [RequireRole]/[RequireMembership] na base
      ↳ PolicyService                             # regra de negócio
```

- `ICommand`, `ICommand<TResult>` e `IQuery<TResult>` herdam `IUseCase`: é por ele que a validação e a autorização tratam qualquer entrada.
- Na presentation: `validation.RunAsync(new CreatePolicyCommand(...), policyService.CreatePolicyAsync, ct)` (ou `ExecuteAsync`, sem retorno).
- Cada service é registrado pela sua interface já embrulhado pelo proxy de autorização: um método novo nunca fica sem checagem.
- O core não valida tokens (gRPC interno); a autenticação é do BFF. As roles continuam checadas no core, consultando a base.

### Organograma

Os grupos da organização formam uma árvore gerida pela própria organização (`edit_organization`): o grupo default
(Direção) é a raiz; novos grupos entram abaixo do grupo pai informado (ou da raiz) e podem ser movidos com seus
subgrupos (sem ciclos). **Decidir (aprovar/rejeitar) mandatos e boletas exige estar num grupo acima de quem fez o pedido**,
além da role (`approve_mandate`/`approve_order`): ramo lateral ou mesmo nível não decidem, ninguém decide o próprio pedido,
e quem não está em grupo algum fica na base do organograma. A equipe interna FIX não decide em organizações clientes.

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
├── domain/          # Entidades e regras próprias do BFF: dashboards (widgets, layout, cores, consulta) e filtros salvos,
│                    # visibilidade privada/pública, SharedResource (quem vê/edita) e interfaces de repositório
├── application/     # Um módulo por entidade: auth, organizations, counterparties, policies, mandates, orders, roles, rules, timeline,
│                    # dashboards, filters, search (catálogo de campos, motor de critérios, agregação, leitura do core com cache)
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
│   ├── elasticsearch/  # Repositórios de dashboards e filtros (índices fix-dashboards, fix-saved-filters)
│   ├── cache/       # RedisCache (falha de Redis vira "sem cache", nunca erro)
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
| `GET, POST /api/organization/members` · `PATCH /:id/desk` · `PUT /:id/rules` · `DELETE /:id` | Membros (alçadas + mesa) |
| `POST /api/organization/owner` {memberId} | Transferir a propriedade (204) |
| `GET, POST /api/organization/groups` · `PATCH /:id/parent` · `PUT /:id/rules` · `POST /:id/members` | Grupos e organograma (grupo pai, alçadas) |
| `GET, POST /api/counterparties[?onlyHomologated]` · `GET, PUT, DELETE /:id` · `PATCH /:id/homologation` | Contrapartes |
| `GET, POST /api/policies` · `GET, PUT, DELETE /:id` · `PUT /:id/limits` | Política |
| `POST /api/policies/:id/submit`, `/approve` (ata), `/versions` | Ciclo de aprovação |
| `POST, PUT, DELETE /api/policies/:id/{axes,bands,instruments}[/:childId]` | Eixos, bandas e instrumentos |
| `GET, POST /api/mandates[?policyId&status]` · `POST /preview` · `GET, PUT, DELETE /:id` · `POST /:id/{approve,reject,close}` | Mandatos |
| `GET, POST /api/orders[?mandateId&approval&confirmation]` · `GET, PUT, DELETE /:id` · `POST /:id/{approve,reject}` | Boletas |
| `POST /api/orders/:id/{confirmation,divergence,refusal,resolve}` | Confirmation (middle office) |
| `GET /api/roles` | Catálogo de roles |
| `GET, POST /api/rules` · `PUT, DELETE /:id` | Rules fixas e alçadas personalizadas da organização |
| `GET /api/timeline?entityType=&entityId=` | Auditoria |
| `GET /api/search/sources` | Catálogo: conjuntos de dados, campos, operadores e opções |
| `POST /api/search/:source` {criteria, filterId, sort, page} | Pesquisa (orders, mandates, counterparties, policies) |
| `POST /api/search/aggregate` {type, query} | Prévia de widget |
| `GET, POST /api/filters[?source]` · `GET, PUT, DELETE /:id` | Filtros salvos |
| `GET, POST /api/dashboards` · `GET, PUT, DELETE /:id` · `POST /:id/duplicate` | Dashboards (template `blank` ou `fix-overview`) |
| `POST /api/dashboards/:id/widgets` · `PUT /widgets/order` · `PUT, DELETE /widgets/:widgetId` · `GET /widgets/:widgetId/data` | Widgets e seus dados |

#### Dashboards, filtros e pesquisa (domínio do BFF)

- **Visibilidade**: `Private` (só o dono) ou `Public` (toda a organização vê). Editam o dono e, se público, quem tem `edit_organization`;
  só o dono muda a visibilidade. Qualquer membro pode **duplicar** um dashboard público para uma cópia privada.
- **Widget**: tipo (`Kpi`, `Column`, `Bar`, `Line`, `Donut`, `Table`), largura 2–12 colunas, altura 120–720 px, cores `#RRGGBB`
  (uma cor ou uma por categoria; sem cores usa a paleta do tema) e consulta: conjunto, medida (count/sum/avg/min/max), dimensão,
  ordem, limite (excedente vira "Outros"), filtro salvo e condições próprias.
- **Dashboard público só usa filtros públicos**; filtro usado por dashboard não pode ser excluído (nem ficar privado se o dashboard for público).
- **Pesquisa**: igualdades suportadas pelas listagens do core (ex.: `approval`, `status`, `mandateId`) vão como parâmetro gRPC; as demais
  condições (contém, entre, maior que...) são aplicadas no BFF. Até 5.000 linhas por conjunto.
- **Cache (Redis)**: as linhas lidas do core ficam em `viz:{org}:rows:{user}:...` por `VIZ_CACHE_TTL_SECONDS` (30 s). Qualquer escrita
  da organização via BFF invalida `viz:{org}:*`; mudanças por outros canais expiram pelo TTL. A membership (roles) também é cacheada (60 s).
- `npm test` roda os testes do domínio e do motor de pesquisa (node:test).

### Web (`frontend/web`)

```
src/
├── domain/          # Tipos por entidade (organization, counterparty, policy, mandate, order, role, rule), permissões e rótulos pt-BR
├── application/     # Stores Pinia (sessão, organização + roles, catálogo de roles/rules, contadores das filas) e provedor da API
├── infrastructure/  # HttpClient (Authorization + X-Organization-Id), APIs REST por entidade, localStorage
└── presentation/    # Router (guards de sessão/tenant/role), layouts, views, componentes e composables
```

Menu lateral com quatro áreas, cada uma com a sua barra de abas:

- **Home**: dashboards personalizáveis (meus e da organização): modelo FIX ou em branco, modo edição com arrastar/soltar,
  largura/altura, editor de widget com prévia ao vivo, cores por série, visão em tabela e dica ao passar o mouse; checklist de configuração.
- **Políticas**: Política de riscos (parâmetros, eixos, bandas, instrumentos, versões, histórico), Mandatos (prévia de enquadramento
  ao vivo), Boletas de hedge, Boletas em aberto (confirmations pendentes/divergentes/recusados) e Fila de aprovação.
- **Usuários**: Membros e grupos (owner, alçadas, mesa, organograma em árvore; "Equipe FIX" na organização interna) e Rules e alçadas
  (CRUD das alçadas personalizadas, papéis fixos e as suas roles).
- **Organização**: Setup da companhia (identificação, orçamento/gatilhos, capacidade, commodities, financeiro), Contrapartes e Timeline.
- Listagens de **Mandatos** e **Boletas** usam a pesquisa do BFF com barra de filtros: condições avulsas e filtros salvos (privados ou públicos).
- Em organização cliente acessada pela equipe interna aparece o aviso "Acesso de suporte FIX".
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
