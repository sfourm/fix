namespace Fix.Storage.Domain.AggregateRoots.Files.Repositories;

/// <summary>Resumo de um tipo de arquivo na organização (cards da tela de uploads).</summary>
public sealed record FileKindSummary(FileKind Kind, int Total, int Processing, int WithErrors, DateTimeOffset? LastUploadedAt);

/// <summary>
/// Arquivos (MongoDB, coleção "files"). Sem unit of work: status e contadores mudam por operações atômicas, porque
/// várias linhas do mesmo arquivo terminam ao mesmo tempo.
/// </summary>
public interface IFileRepository
{
    Task<File?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<File>> ListAsync(Guid organizationId, FileKind? kind, int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<FileKindSummary>> SummaryAsync(Guid organizationId, CancellationToken cancellationToken);

    Task AddAsync(File file, CancellationToken cancellationToken);

    /// <summary>Received → Processing com o total de linhas. Idempotente: só um consumidor sai de Received.</summary>
    Task<bool> StartProcessingAsync(Guid id, int totalLines, CancellationToken cancellationToken);

    Task FailAsync(Guid id, string error, DateTimeOffset at, CancellationToken cancellationToken);

    /// <summary>Soma uma linha processada; quando todas terminam, grava o status final. Devolve o arquivo atualizado.</summary>
    Task<File> RegisterLineResultAsync(Guid id, bool succeeded, DateTimeOffset at, CancellationToken cancellationToken);
}
