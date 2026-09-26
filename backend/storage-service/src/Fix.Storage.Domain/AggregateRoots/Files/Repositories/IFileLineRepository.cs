namespace Fix.Storage.Domain.AggregateRoots.Files.Repositories;

/// <summary>Linhas dos arquivos (MongoDB, coleção "file_lines"; única por arquivo + número).</summary>
public interface IFileLineRepository
{
    Task<FileLine?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<FileLine>> ListAsync(Guid fileId, FileLineStatus? status, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Grava as linhas lidas. Idempotente: numa reentrega, as já gravadas ficam como estão. Devolve as linhas que
    /// valem (as recém-gravadas ou as que já existiam), na ordem do arquivo.
    /// </summary>
    Task<IReadOnlyList<FileLine>> AddManyAsync(IReadOnlyList<FileLine> lines, CancellationToken cancellationToken);

    Task UpdateAsync(FileLine line, CancellationToken cancellationToken);
}
