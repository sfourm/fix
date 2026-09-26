using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using MongoDB.Driver;

namespace Fix.Storage.Infrastructure.Persistence.Repositories;

internal sealed class FileLineRepository(StorageDbContext dbContext) : IFileLineRepository
{
    private const int DuplicateKey = 11000;

    public async Task<FileLine?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.FileLines.Find(l => l.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedList<FileLine>> ListAsync(Guid fileId, FileLineStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var filter = Builders<FileLine>.Filter.Eq(l => l.FileId, fileId);
        if (status is { } s)
        {
            filter &= Builders<FileLine>.Filter.Eq(l => l.Status, s);
        }

        var total = await dbContext.FileLines.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await dbContext.FileLines.Find(filter)
            .SortBy(l => l.Number)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<FileLine>(items, page, pageSize, (int)total);
    }

    public async Task<IReadOnlyList<FileLine>> AddManyAsync(IReadOnlyList<FileLine> lines, CancellationToken cancellationToken)
    {
        var duplicated = false;
        foreach (var chunk in lines.Chunk(1000))
        {
            try
            {
                await dbContext.FileLines.InsertManyAsync(chunk, new InsertManyOptions { IsOrdered = false }, cancellationToken);
            }
            catch (MongoBulkWriteException exception) when (exception.WriteErrors.All(e => e.Code == DuplicateKey))
            {
                // Reentrega do evento: as linhas já gravadas (índice único arquivo + número) ficam como estão.
                duplicated = true;
            }
        }

        if (!duplicated || lines.Count == 0)
        {
            return lines;
        }

        return await dbContext.FileLines.Find(l => l.FileId == lines[0].FileId).SortBy(l => l.Number).ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(FileLine line, CancellationToken cancellationToken) =>
        dbContext.FileLines.ReplaceOneAsync(l => l.Id == line.Id, line, cancellationToken: cancellationToken);
}
