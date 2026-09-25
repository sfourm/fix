# Contratos gRPC (`protos/`)

## Organização

- Fonte única da comunicação BFF ↔ core. Pacote `fix.v1`; um arquivo por módulo:
  `auth`, `organizations`, `counterparties`, `policies`, `mandates`, `orders`, `roles`, `rules`, `timeline`, e `common`.
- `common.proto` define `RequestContext` (`user_id`, `organization_id`) e mensagens de paginação.

## Quem consome

- **core-service** compila os `.proto` como servidor (`Fix.Presentation.csproj`: `Protobuf Include="..\..\..\..\protos\**\*.proto"`,
  namespace gerado `Fix.Contracts.V1`).
- **BFF** carrega os `.proto` em runtime (`GRPC_CONTRACTS=files`, `PROTOS_DIR` padrão = `protos/` do repositório) ou pergunta
  ao core via server reflection (`GRPC_CONTRACTS=reflection`).

## Regras

- Toda RPC de negócio recebe `context` (`RequestContext`); o core usa `user_id`/`organization_id` para autorizar.
- Enums do contrato usam prefixo do tipo (`COMMODITY_RAW_SUGAR`); o BFF os converte para PascalCase do domínio (`RawSugar`)
  em `frontend/bff/src/infrastructure/grpc/mappers`.
- Evolução compatível: adicione campos com números novos; não reutilize nem renumere campos existentes.
- RPC que não devolve dado usa resposta vazia (o BFF responde `204`).

## Mudou um contrato? Atualize junto

1. `protos/<módulo>.proto`.
2. core: método no `*GrpcService` + `*ContractMapper`.
3. BFF: gateway em `infrastructure/grpc/gateways` + mapper + port/serviço na application.
4. web: tipo em `domain` e função em `infrastructure/api`, se exposto.
