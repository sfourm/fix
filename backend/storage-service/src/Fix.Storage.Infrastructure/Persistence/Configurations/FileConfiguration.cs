using MongoDB.Bson.Serialization;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Infrastructure.Persistence.Configurations;

/// <summary>Coleção "files". Status e contadores são gravados pelos updates atômicos do repositório.</summary>
internal static class FileConfiguration
{
    public static void Configure()
    {
        BsonClassMap.TryRegisterClassMap<AggregateRoot>(map => map.SetIsRootClass(false));
        BsonClassMap.TryRegisterClassMap<File>(map =>
        {
            map.AutoMap();
            // Calculados: não vão para o banco.
            map.UnmapProperty(f => f.IsFinished);
            map.UnmapProperty(f => f.ProgressPercent);
        });
    }
}
