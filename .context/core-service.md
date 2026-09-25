# core-service (`backend/core-service`)

.NET 10 com `Nullable` e `ImplicitUsings` ligados (`Directory.Build.props`). Solução `Fix.slnx`. Expõe gRPC em `:5098`
(HTTP/2, com server reflection). EF Core 10 + Npgsql sobre PostgreSQL.

## Projetos e camadas

| Projeto | Responsabilidade | Estrutura |
| --- | --- | --- |
| `Fix.Domain` | Regras de negócio puras | `AggregateRoots/<Agregado>/{Entities,Enums,ValueObjects,Repositories,Timeline,Statics,Templates}`, `Services` (ex.: `MandateCompliance`), `Common` (`Abstractions`: `AggregateRoot`, `Entity`, `ValueObject`, `DomainException`, `Timeline`, `PagedList`; `DeterministicGuid`, `DomainGuard`, value objects comuns) |
| `Fix.Application` | Casos de uso e autorização | `UseCases/<Módulo>/{Commands,Queries,Services,Validators,Mappers,Dtos}`; `Common/{Interfaces,Authorization,Attributes,Exceptions,Models,Statics}` |
| `Fix.Infrastructure` | Persistência e integrações | `Persistence/{Configurations,Repositories,Migrations,Auditing,Telemetry}`, `FixDbContext`, `UnitOfWork`, `DatabaseInitializer`, `Identity` |
| `Fix.Presentation` | Entrada gRPC | `Services/*GrpcService`, `Mappers/*ContractMapper`, `Interceptors/ExceptionInterceptor`, `UseCases/UseCaseExecution`, `Telemetry` |
| `tests/Fix.Domain.Tests` | Testes de domínio (xUnit) | Um arquivo por agregado/regra |

Dependências: Presentation → Application + Infrastructure; Infrastructure → Application + Domain; Application → Domain.
O domínio não depende de nada.

## Fluxo de um caso de uso

```
gRPC service (Presentation)
  → IValidationFactory.ValidateAsync(command)       # FluentValidation: resolve IValidator<T> para qualquer IUseCase
  → I<Nome>Service.<Ação>Async(command)             # interface em Fix.Application.Common.Interfaces.UseCases
      ↳ UseCaseGuardProxy → UseCaseGuard            # DispatchProxy: define o RequestContext e checa [RequireRole]/[RequireMembership] na base
      ↳ <Nome>Service                               # orquestra domínio + repositórios + UnitOfWork
```

Na presentation:

```csharp
var dto = await validation.RunAsync(new CreatePolicyCommand(...), policyService.CreatePolicyAsync, context.CancellationToken);
await validation.ExecuteAsync(new DeletePolicyCommand(...), policyService.DeletePolicyAsync, context.CancellationToken);
```

## Padrões

### Commands e queries
- `sealed record` com `UserId` e `OrganizationId` primeiro; implementam `ICommand`, `ICommand<T>` ou `IQuery<T>` (todos herdam
  `IUseCase`) e `IOrganizationRequest` / `IUserRequest`.
- Autorização declarativa no próprio record: `[RequireRole(RoleCodes.ManageCounterparties)]` ou `[RequireMembership]`.
  Se o record tiver uma propriedade chamada `RoleCodes`, qualifique o atributo com `global::Fix.Domain.AggregateRoots.Roles.RoleCodes`.

### Services
- **Sem** `ICommandHandler`/`IQueryHandler` nem mediator. Uma interface `I<Nome>Service` por módulo, com **métodos nomeados**
  (`CreatePolicyAsync`, `UpdatePolicyLimitsAsync`, `ApproveMandateAsync`...).
- Classe `internal sealed` com construtor primário; seções `// ---------- Commands ----------` e `// ---------- Queries ----------`.
- Registrada pela interface já embrulhada pelo `UseCaseGuardProxy`: método novo nunca fica sem checagem de autorização.
- Termina commands com `unitOfWork.SaveChangesAsync(ct)` e devolve DTO via mapper.

### Validação
- Um `<Command>Validator` (FluentValidation) por entrada: formato, tamanhos, obrigatórios. Invariantes de negócio ficam no domínio.

### Domínio
- Agregados ricos com fábricas estáticas (`Create`, `CreateInternal`, `CreateCustom`) e métodos de comportamento; nada de setters
  públicos. `DomainGuard` para pré-condições; value objects (`Name`, `Percentage`...).
- Violação de regra → `DomainException` com mensagem em **pt-BR pronta para o usuário**
  (ex.: "A boleta excede o saldo do mandato (autorizado 300, consumido 200, boleta 150).").
- Ids de dados de sistema (roles, rules base, organização FIX) com `DeterministicGuid.From("...")` para seeds estáveis.

### DTOs e mapeamento
- DTOs `record` na Application (`UseCases/<Módulo>/Dtos`), mappers estáticos `ToDto()`.
- A Presentation mapeia DTO ↔ contrato em `Mappers/*ContractMapper` (enums de contrato ↔ enums de domínio em `ContractParsing`).

### Namespaces
Seguem o módulo, não a pasta física:
- `Fix.Application.<Módulo>.{Commands,Queries,Services,Validators,Mappers,Dtos}` (pasta `UseCases/<Módulo>`)
- `Fix.Application.Abstractions.*` (contratos da Application: contexto, mensageria, exceções, validação, autorização)
- `Fix.Application.Common.Interfaces.UseCases` (interfaces dos services)
- `Fix.Domain.AggregateRoots.<Agregado>` (entidades do agregado) e `.Repositories`

### Erros → gRPC (`ExceptionInterceptor`)

| Exceção | Status gRPC | HTTP no BFF |
| --- | --- | --- |
| `ValidationException` (FluentValidation) | `INVALID_ARGUMENT` + trailer `validation-errors` (JSON por campo) | 400 |
| `BadRequestException` | `INVALID_ARGUMENT` | 400 |
| `UnauthenticatedException` | `UNAUTHENTICATED` | 401 |
| `ForbiddenException` | `PERMISSION_DENIED` | 403 |
| `NotFoundException` | `NOT_FOUND` | 404 |
| `ConflictException` | `ALREADY_EXISTS` | 409 |
| `DomainException` | `FAILED_PRECONDITION` | 422 |
| outras | `INTERNAL` (logada) | 500 |

## Persistência

- EF Core com convenção snake_case; configurações em `Persistence/Configurations` (uma por entidade).
- Repositórios por agregado (interfaces no domínio, implementação em `Persistence/Repositories`); `IUnitOfWork` salva.
- Seeds com `HasData`. `DatabaseInitializer` aplica migrations e garante a organização FIX + super administrador no startup.
- **Migrations**:
  `dotnet ef migrations add <Nome> -p src/Fix.Infrastructure -s src/Fix.Presentation -o Persistence/Migrations`.
  Quando a mudança transforma dados existentes, a migration leva SQL manual **na ordem certa**: criar colunas → converter/remapear
  dados → apagar/restringir (FKs `Restrict` quebram se apagar antes). Faça `pg_dump` antes de aplicar numa base com dados.

## Auditoria e timeline

- `created_at`, `updated_at`, `author_created`, `author_updated` são **shadow properties**: o domínio não as conhece.
- O `AuditingInterceptor` preenche esses campos e grava cada mudança (Created/Updated/Deleted, com old/new) na timeline do
  agregado dono (`OrganizationTimeline`, `PolicyTimeline`, `MandateTimeline`..., tabela `timelines` via TPH).
- Mudanças em entidades filhas entram na timeline do agregado com o nome da entidade como prefixo (`PolicyAxis.Title`).

## Telemetria

`Telemetry/TelemetryExtensions.AddBackendTelemetry`: ASP.NET Core/gRPC, HttpClient e Npgsql (traces e métricas), exportando OTLP
para `OpenTelemetry:OtlpEndpoint`. Spans SQL são nomeados `OPERAÇÃO tabela` por `Persistence/Telemetry/SqlCommandTelemetry.cs`.
Nome do serviço: `fix-backend`. Detalhes em [observabilidade.md](observabilidade.md).

## Comandos

```bash
cd backend/core-service
docker compose up -d                        # PostgreSQL :5432 (fix/fix), projeto compose fix-core-service
dotnet run --project src/Fix.Presentation   # gRPC :5098; migrations e seed em Development
dotnet test                                 # testes de domínio
grpcurl -plaintext localhost:5098 list      # contratos via server reflection
```
