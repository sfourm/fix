# Web (`frontend/web`)

Vue 3 (`<script setup lang="ts">`) + Vite + TypeScript + Pinia + Vue Router. Em dev o Vite faz proxy de `/api` para o BFF
(`BFF_URL`, padrão `http://localhost:3000`).

## Estrutura

```
src/
├── domain/          # tipos por entidade, Permission (roles), rótulos pt-BR (labels.ts), catálogo de roles/rules, busca, dashboards
├── application/     # stores Pinia e provedor da API (useApi)
│   └── stores/      # session, organization (tenant + roles efetivas + flags de suporte), role, rule, contadores das filas
├── infrastructure/
│   ├── http/        # HttpClient (Authorization + X-Organization-Id), ApiError, errorMessage
│   ├── api/         # uma API REST por entidade (organization.api.ts, rule.api.ts...)
│   └── storage/     # localStorage
└── presentation/
    ├── router/      # rotas e guards (sessão, tenant, permissão via meta)
    ├── layouts/     # AppLayout: menu lateral + barra de abas da área
    ├── navigation.ts# áreas e abas
    ├── views/       # por área: home, policies, mandates, orders, approvals, organizations, setup, timeline, auth
    ├── components/  # base (PageHeader, BaseModal, ConfirmDialog, StatusBadge, StateBlock, PaginationBar, ToastHost, NumberInput,
    │                # TimelineList, DecisionModal) e por tema (dashboard, filters, mandate, order, policy, viz)
    ├── composables/ # useSubmit, useConfirm, useToast...
    └── styles/
```

## Navegação

Menu lateral com quatro áreas; cada área tem sua barra de abas (`presentation/navigation.ts`):

| Área | Abas |
| --- | --- |
| Home | Dashboards personalizáveis e checklist de configuração |
| Políticas | Política de riscos · Mandatos · Boletas de hedge · Boletas em aberto · Fila de aprovação |
| Usuários | Membros e grupos · Rules e alçadas |
| Organização | Setup da companhia · Contrapartes · Timeline |

Abas com contador (boletas em aberto, fila de aprovação) usam os contadores da store.

## Permissões na UI

- Rotas declaram a permissão em `meta: page(Permission.X)`; o guard redireciona quem não tem.
- Botões e ações usam `organization.can(Permission.X)`. Isso só **esconde**: quem autoriza é o core.
- Em organização cliente acessada pela equipe interna (`organization.internalAccess`), o layout mostra "Acesso de suporte FIX".
- Na organização FIX (`organization.isInternalOrganization`), membros aparecem como "Equipe FIX" e as alçadas são fixas.

## Padrões de código

- Views finas: carregam dados via store/API, usam componentes base e composables.
- Formulários: `useSubmit()` (estado de envio e erro), `useConfirm()` para confirmações destrutivas, `useToast()` para feedback.
- Erros da API: `errorMessage(e)` (mensagens pt-BR vindas do core/BFF; validação por campo quando houver).
- Textos da UI em português; rótulos de enums sempre via `domain/labels.ts`.
- O web **não** gera `traceparent`: sem um SDK que exporte spans do browser, o trace ficaria sem raiz. Correlação via header
  `x-trace-id` da resposta.

## Dashboards e gráficos

- Home com dashboards (meus e da organização): modelo FIX ou em branco, modo edição com arrastar/soltar, largura/altura,
  editor de widget com prévia ao vivo, cores por série, visão em tabela e tooltip.
- Gráficos em SVG próprio (`components/viz`), seguindo regras de dataviz: cor segue a entidade (ordem fixa de categorias),
  um eixo só (nunca dois eixos Y), legenda para ≥ 2 séries, tooltip no hover, visão em tabela, cores de status reservadas.
- Listagens de Mandatos e Boletas usam a pesquisa do BFF com barra de filtros (condições avulsas e filtros salvos).

## Comandos

```bash
cd frontend/web
npm install
npm run dev         # http://localhost:5173
npm run typecheck   # vue-tsc
npm run build
```
