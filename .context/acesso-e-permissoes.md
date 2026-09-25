# Acesso, tenants e permissões

## Tenants

- Tenant = **organização**. Todo request de negócio carrega a organização: header `X-Organization-Id` no web → BFF →
  `RequestContext.organization_id` no gRPC.
- O usuário escolhe a organização após o login (`/organizations`); pode ser membro de várias.

## Roles e rules

- **Roles**: 22 permissões atômicas (`view_policy`, `edit_policy`, `approve_mandate`, `manage_counterparties`, `view_users`,
  `edit_organization`...). Código em `Fix.Domain/AggregateRoots/Roles/Statics`.
- **Rules**: conjuntos de roles atribuíveis. Roles efetivas de um membro = rule base (owner/user) + alçadas diretas + alçadas
  herdadas dos grupos a que pertence.
- O web consulta as roles efetivas em `GET /api/organization/roles` apenas para exibir/ocultar ações. **Quem autoriza é o core**,
  consultando a base a cada caso de uso (`[RequireRole]` / `[RequireMembership]`).

## Organização interna FIX

- Criada no startup do core (id determinístico `organization:fix`), com o **super administrador** configurado em
  `Seed:SuperAdministrator` (dev: `admin@fix.local` / `Admin@12345`).
- O super administrador adiciona e remove **administradores**. Os papéis `super_administrador` e `administrador` são internos:
  nunca aparecem nem são atribuíveis em organizações clientes.
- Na tela de membros a organização FIX aparece como "Equipe FIX".

## Suporte FIX em organizações clientes

- Super administrador e administradores veem todas as organizações (seção "Suporte FIX" na escolha de organização).
- Recebem `SystemRoles.Staff`: **visualizam e editam** (setup, membros, alçadas, política...), mas **nunca decidem**.
- Roles de decisão (`SystemRoles.Decisions`), nunca dadas ao suporte:
  `approve_policy`, `approve_mandate`, `approve_exception`, `approve_order`, `manage_confirmation`, `self_approve`.
- A UI mostra o aviso "Acesso de suporte FIX" nessas organizações.

## Organização cliente: owner, user e alçadas

- Sempre existe **exatamente um owner** (todas as roles). Quem cria a organização vira owner. Não pode ser removido; a
  propriedade é **transferível** pelo próprio owner ou pelo suporte (o antigo owner vira user).
- Os demais membros são **user** (leitura).
- Permissões além da leitura vêm de **alçadas personalizadas**, criadas pela própria organização em Usuários › Rules e alçadas
  (`/access`) a partir das roles disponíveis, e atribuídas a membros e grupos. Editar uma alçada vale na hora para quem a tem;
  excluir retira de todos.
- `Gestor (Diretoria)`, `Operador (Mesa)` e `Middle office (Controle de riscos)` são **modelos editáveis** copiados para cada
  organização nova.
- Na UI, roles de decisão aparecem destacadas; marcar uma ação marca junto a visualização da mesma área.

## Organograma e decisões

- Os grupos formam uma **árvore** gerida pela organização (`edit_organization`). O grupo raiz é "Direção"; grupos novos entram
  abaixo do pai informado; mover um grupo leva os subgrupos (sem ciclos; a raiz não se move).
- **Decidir** (aprovar/rejeitar) mandatos e boletas exige a role correspondente **e** estar num grupo **acima** de quem fez o pedido.
  Ramo lateral ou mesmo nível não decidem; ninguém decide o próprio pedido; quem não está em grupo fica na base.
- Implementação: `Organization.IsAboveInOrgChart` e `OrgChartApproval` no core.
