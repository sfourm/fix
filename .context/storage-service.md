# storage-service (`backend/storage-service`)

Serviço .NET 10 (gRPC) dos **uploads**: recebe arquivos, guarda o original no S3, lê as planilhas e executa cada linha no
core-service. Segue as mesmas camadas e padrões do core ([core-service.md](core-service.md)).

## Tipos de upload

| Tipo | Processado? | O que cada linha faz no core | Regra para enviar / ver |
| --- | --- | --- | --- |
| Usuários | sim | `AddMember` (conta existente) + cargo, mesa e grupo | `create_user` / `view_user` |
| Políticas | sim — **só criação** | `CreatePolicy` (rascunho, opcionalmente com o modelo FIX) | `create_policy` / `view_policy` |
| Mandatos | sim | `IssueMandate` no eixo da política | `create_mandate` / `view_mandate` |
| Boletas | sim | `RegisterOrder` (com ou sem mandato) | `create_order` / `view_order` |
| Documentos | não (só armazena) | — | qualquer membro |

Modelos (colunas, obrigatórias, exemplos) em `Domain/AggregateRoots/Files/Templates/FileTemplates.cs`; aceitam `.csv`
(`;`, `,` ou tab; UTF-8 ou Latin-1 do Excel), `.xlsx` e `.xml` (`<linhas><linha><coluna>…`). Cabeçalho sem acento ou com,
maiúsculas indiferentes e alguns apelidos (`E-mail` → `email`). Valores pt-BR: `16,42`, `25/09/2026`, `sim/não`, opções
sem acento (`logística`).

## Fluxo

```
BFF ──UploadFile (gRPC)──▶ valida (regra do tipo, extensão, cabeçalho, ≤ 20 MB, ≤ 20 mil linhas)
                           ▶ grava no S3 ▶ registra em Mongo (files) ▶ publica FileUpload (file.uploaded)
FileUploadConsumer ─▶ lê do S3, grava as linhas (file_lines) ▶ publica uma mensagem por linha (file.line.received)
FileLineConsumer  ─▶ executa a linha no core em nome de quem enviou ▶ grava o resultado ▶ publica file.progress
BFF ──consome file.progress (fila exclusiva)──▶ SSE /api/files/events ──▶ web (progresso ao vivo)
```

- **Uma linha não afeta as outras**: cada uma é uma mensagem; erro de negócio/preenchimento vira falha só dela, com a
  mensagem do core. Core fora do ar: a linha volta para a fila (3 tentativas) antes de falhar.
- **Idempotência**: reentrega do `FileUpload` não relê o arquivo (só sai de `Received` uma vez; linhas únicas por arquivo +
  número); reentrega de uma linha já processada é ignorada.
- **Ordem**: `RabbitMq:LinePrefetch = 1` executa as linhas uma a uma (evita disputa na numeração MD-/HX- do core).
- Contadores do arquivo mudam por `$inc` atômico no Mongo; a última linha fecha o status (`Completed` ou
  `CompletedWithErrors`).
- O navegador **nunca acessa o S3**: o download passa pelo BFF (`GetFileContent`).

## Camadas

- **Domain**: agregado `Files` (`File`, `FileLine`, enums, `IFileRepository`/`IFileLineRepository`, modelos).
- **Application**: `UseCases/Files` (commands, queries, dtos, mappers, services `FileService` e `FileProcessingService`,
  validators, eventos); `Common` com `UseCaseGuard` (membership) e `FilePermissions` (regra por tipo).
- **Infrastructure**: `Persistence` (Mongo: `StorageDbContext`, configurações de mapeamento, repositórios, índices na
  subida), `Storage` (S3 via AWSSDK), `Messaging` (RabbitMQ: exchange `fix.files`, filas `storage.file-uploaded`,
  `storage.file-lines`, dead-letter `storage.files.dead`), `Spreadsheets` (CSV/XLSX/XML), `CoreService` (clientes gRPC do
  core, leitura dos valores pt-BR e execução das linhas). Compila os `protos/` (core como cliente, `files.proto` como
  servidor).
- **Presentation**: `FileGrpcService`, `ContractParsing`, interceptor de erros (mesmo mapeamento do core).

## Rodando localmente

```bash
cd backend/storage-service && docker compose up -d     # RabbitMQ :5672 (painel :15672, fix/fix), Mongo :27018, S3 (RustFS) :9000
dotnet run --project src/Fix.Storage.Presentation       # gRPC :5099 (precisa do core em :5098)
dotnet test Fix.Storage.slnx                            # modelos, leitores, valores pt-BR e processamento
```

## No cluster

Chart `fix`: `fix-storage-service`, `fix-mongodb`, `fix-rabbitmq` (StatefulSets com volume). Arquivos no **bucket S3 da AWS**
criado pelo Terraform (`infra/terraform/s3.tf`, `objectStorage.provider: aws`), com um usuário IAM restrito ao bucket;
bucket, região e chave chegam ao Secret `fix-secrets` pelo `fix-sync`. Senhas internas do Mongo e do RabbitMQ são geradas
pelo chart (Secret `fix-storage-secrets`, preservado nos upgrades). `objectStorage.provider: internal` usa RustFS no cluster.
