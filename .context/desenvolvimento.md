# Desenvolvimento

## Rodando localmente

```bash
# 0. Observabilidade
cd observability && docker compose up -d                                                      # Grafana :3001, Jaeger :16686, Prometheus :9090

# 1. Banco + core
cd backend/core-service && docker compose up -d && dotnet run --project src/Fix.Presentation     # PostgreSQL :5432 (fix/fix), gRPC :5098

# 1b. Uploads (outro terminal; opcional)
cd backend/storage-service && docker compose up -d && dotnet run --project src/Fix.Storage.Presentation  # RabbitMQ, Mongo :27018, S3 :9000, gRPC :5099

# 2. BFF (outro terminal)
cd frontend/bff && docker compose up -d && npm install && npm run dev                          # Elasticsearch :9200, Redis :6379, BFF :3000

# 3. Web (outro terminal)
cd frontend/web && npm install && npm run dev                                                   # http://localhost:5173
```

Em Development o core aplica migrations e seeds no startup, incluindo a organização FIX e o super administrador
(`admin@fix.local` / `Admin@12345`). Para usar como cliente: crie uma conta e uma organização (quem cria vira owner).

## Testes e verificações

| Projeto | Comando | O que cobre |
| --- | --- | --- |
| core-service | `dotnet test` | Regras de domínio (xUnit) |
| storage-service | `dotnet test Fix.Storage.slnx` | Modelos das planilhas, leitores CSV/XLSX/XML, valores pt-BR, processamento das linhas |
| BFF | `npm test` · `npm run typecheck` | Domínio de dashboards/filtros e motor de pesquisa (node:test) · tipos |
| web | `npm run typecheck` · `npm run build` | Tipos (vue-tsc) e build |

Mudança de comportamento visível deve ser verificada também com o fluxo rodando no navegador.

## Receita: funcionalidade ponta a ponta

1. **Domínio (core)**: comportamento no agregado (+ `DomainException` com mensagem pt-BR), value objects, repositório se preciso;
   teste em `tests/Fix.Domain.Tests`.
2. **Application**: `Command`/`Query` (record + `[RequireRole]`/`[RequireMembership]`), `Validator`, método nomeado na interface
   `I<Nome>Service` e no service, DTO + mapper.
3. **Infrastructure**: configuração EF / repositório; migration se o modelo mudou (com conversão de dados se necessário).
4. **Contrato**: RPC e mensagens em `protos/<módulo>.proto` (com `context`).
5. **Presentation (core)**: método no `*GrpcService` com `validation.RunAsync(...)`/`ExecuteAsync(...)` e mapper do contrato.
6. **BFF**: command/query + response, port, gateway gRPC + mapper, service, rota com schema zod (`z.guid()` para ids) e
   `invalidateOnWrite` se altera dados exibidos em dashboards/pesquisa.
7. **Web**: tipo em `domain`, função em `infrastructure/api`, store se houver estado compartilhado, view/componente,
   permissão na rota e nos botões.
8. **Verificar**: testes e typechecks dos três projetos + fluxo no navegador; conferir o trace no Jaeger se envolver novas chamadas.

## Nova API no backend

1. Criar `backend/<nome>-service/` com solução própria, `Directory.Build.props`, `dotnet-tools.json` e `docker-compose.yml`
   com `name: fix-<nome>-service` e volumes com `name:` explícito.
2. Seguir as mesmas camadas e padrões do `core-service` (ver [core-service.md](core-service.md)).
3. Contratos em `protos/` (pacote próprio ou `fix.v1` conforme o domínio) e telemetria com `service.name` próprio.
4. Registrar no README, nesta pasta e nos dashboards se fizer sentido.

## Convenções de trabalho

- Siga o estilo do código ao redor: densidade de comentários, nomes, idioma (código em inglês; textos e comentários em pt-BR).
- Não levar preocupações de apresentação (dashboards, filtros, layout) para o core, nem regra de negócio para BFF/web.
- Autorização nova = role/atributo no core; o web apenas reflete.
- Commits e push só quando pedido. Mensagens descritivas; não pular hooks.
- Antes de operações destrutivas no banco (migrations de conversão, mover volumes), faça `pg_dump`.
- Mantenha esta pasta `.context` atualizada junto com mudanças de regra, arquitetura ou convenção.

## Armadilhas conhecidas

- `DeterministicGuid` gera GUIDs fora do RFC-4122: valide ids com `z.guid()` no BFF.
- Migration com FK `Restrict`: converter/remapear dados **antes** de `DeleteData`.
- Mover um `docker-compose.yml` muda o nome do projeto compose e dos volumes: mantenha `name:` fixo e volumes nomeados
  (o Postgres do core usa o volume `fix-core-service-postgres-data`).
- BFF ESM + OpenTelemetry: CommonJS importado de ESM não é instrumentado sem pré-carregar (`instrumentation.ts`); `tsx watch`
  impede o patch — use o script `dev` existente.
- Windows: parar o `npm` não mata o `node` filho; confira as portas (3000, 5098, 5173) antes de reiniciar.
- A extensão C# do VS Code mantém lock em `backend/*/src`: pare o language server antes de mover/renomear pastas.
- Jaeger guarda traces em memória: reiniciar o container apaga o histórico.
