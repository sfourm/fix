using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Infrastructure.Options;
using Fix.Storage.Infrastructure.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Infrastructure.Persistence;

/// <summary>Na subida: índices do MongoDB e, no ambiente local, o bucket do MinIO.</summary>
internal sealed class DatabaseInitializer(
    StorageDbContext dbContext,
    S3ObjectStorage objectStorage,
    IOptions<S3Options> s3Options,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Files.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<File>(Builders<File>.IndexKeys
                    .Ascending(f => f.OrganizationId).Ascending(f => f.Kind).Descending(f => f.UploadedAt)),
            ], cancellationToken);
            await dbContext.FileLines.Indexes.CreateManyAsync(
            [
                // Única: reler o arquivo (reentrega do FileUpload) não duplica as linhas.
                new CreateIndexModel<FileLine>(
                    Builders<FileLine>.IndexKeys.Ascending(l => l.FileId).Ascending(l => l.Number),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<FileLine>(Builders<FileLine>.IndexKeys
                    .Ascending(l => l.FileId).Ascending(l => l.Status).Ascending(l => l.Number)),
            ], cancellationToken);

            if (s3Options.Value.CreateBucket)
            {
                await objectStorage.EnsureBucketAsync(cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Não derruba o serviço: o erro aparece de novo (e no log) na primeira operação.
            logger.LogError(exception, "Falha ao preparar MongoDB/S3 na subida");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
