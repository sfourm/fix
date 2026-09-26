using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Infrastructure.Persistence.Repositories;

/// <summary>Status e contadores mudam por updates atômicos (as mesmas regras de <see cref="File"/>).</summary>
internal sealed class FileRepository(StorageDbContext dbContext) : IFileRepository
{
    private static readonly FilterDefinitionBuilder<File> Filter = Builders<File>.Filter;
    private static readonly UpdateDefinitionBuilder<File> Update = Builders<File>.Update;

    public async Task<File?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Files.Find(f => f.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedList<File>> ListAsync(Guid organizationId, FileKind? kind, int page, int pageSize, CancellationToken cancellationToken)
    {
        var filter = Filter.Eq(f => f.OrganizationId, organizationId);
        if (kind is { } k)
        {
            filter &= Filter.Eq(f => f.Kind, k);
        }

        var total = await dbContext.Files.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await dbContext.Files.Find(filter)
            .SortByDescending(f => f.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<File>(items, page, pageSize, (int)total);
    }

    public async Task<IReadOnlyList<FileKindSummary>> SummaryAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        static BsonDocument CountWhen(params FileStatus[] statuses) => new("$sum", new BsonDocument("$cond", new BsonArray
        {
            new BsonDocument("$in", new BsonArray { "$status", new BsonArray(statuses.Select(s => s.ToString())) }),
            1,
            0,
        }));

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("organizationId", new BsonBinaryData(organizationId, GuidRepresentation.Standard))),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$kind" },
                { "total", new BsonDocument("$sum", 1) },
                { "processing", CountWhen(FileStatus.Received, FileStatus.Processing) },
                { "withErrors", CountWhen(FileStatus.CompletedWithErrors, FileStatus.Failed) },
                { "last", new BsonDocument("$max", "$uploadedAt") },
            }),
        };

        var rows = await dbContext.Files.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);
        return
        [
            .. rows
                .Where(r => r["_id"].IsString && Enum.TryParse<FileKind>(r["_id"].AsString, out _))
                .Select(r => new FileKindSummary(
                    Enum.Parse<FileKind>(r["_id"].AsString),
                    r["total"].ToInt32(),
                    r["processing"].ToInt32(),
                    r["withErrors"].ToInt32(),
                    r["last"].IsBsonNull ? null : new DateTimeOffset(r["last"].ToUniversalTime(), TimeSpan.Zero))),
        ];
    }

    public Task AddAsync(File file, CancellationToken cancellationToken) =>
        dbContext.Files.InsertOneAsync(file, cancellationToken: cancellationToken);

    public async Task<bool> StartProcessingAsync(Guid id, int totalLines, CancellationToken cancellationToken)
    {
        var result = await dbContext.Files.UpdateOneAsync(
            Filter.Eq(f => f.Id, id) & Filter.Eq(f => f.Status, FileStatus.Received),
            Update.Set(f => f.Status, FileStatus.Processing).Set(f => f.TotalLines, totalLines),
            cancellationToken: cancellationToken);
        return result.ModifiedCount == 1;
    }

    public Task FailAsync(Guid id, string error, DateTimeOffset at, CancellationToken cancellationToken) =>
        dbContext.Files.UpdateOneAsync(
            Filter.Eq(f => f.Id, id),
            Update.Set(f => f.Status, FileStatus.Failed).Set(f => f.Error, error).Set(f => f.FinishedAt, at),
            cancellationToken: cancellationToken);

    public async Task<File> RegisterLineResultAsync(Guid id, bool succeeded, DateTimeOffset at, CancellationToken cancellationToken)
    {
        // $inc atômico: várias linhas podem terminar ao mesmo tempo.
        var increment = Update.Inc(f => f.ProcessedLines, 1);
        increment = succeeded ? increment.Inc(f => f.SucceededLines, 1) : increment.Inc(f => f.FailedLines, 1);
        var file = await dbContext.Files.FindOneAndUpdateAsync(
            Filter.Eq(f => f.Id, id),
            increment,
            new FindOneAndUpdateOptions<File> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

        if (file.Status != FileStatus.Processing || file.ProcessedLines < file.TotalLines)
        {
            return file;
        }

        // Última linha: fecha o arquivo (o filtro no status garante que só uma chamada fecha).
        return await dbContext.Files.FindOneAndUpdateAsync(
            Filter.Eq(f => f.Id, id) & Filter.Eq(f => f.Status, FileStatus.Processing),
            Update.Set(f => f.Status, File.FinalStatus(file.FailedLines)).Set(f => f.FinishedAt, at),
            new FindOneAndUpdateOptions<File> { ReturnDocument = ReturnDocument.After },
            cancellationToken) ?? file;
    }
}
