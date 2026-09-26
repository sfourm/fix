using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Infrastructure.Options;
using Fix.Storage.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Infrastructure.Persistence;

/// <summary>Banco do storage (MongoDB): coleções "files" e "file_lines". Os mapeamentos ficam em Configurations.</summary>
internal sealed class StorageDbContext
{
    public StorageDbContext(IOptions<MongoOptions> options)
    {
        MongoConfiguration.Apply();
        var database = new MongoClient(options.Value.ConnectionString).GetDatabase(options.Value.Database);
        Files = database.GetCollection<File>("files");
        FileLines = database.GetCollection<FileLine>("file_lines");
    }

    public IMongoCollection<File> Files { get; }

    public IMongoCollection<FileLine> FileLines { get; }
}
