using Fix.Storage.Domain.AggregateRoots.Files;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Fix.Storage.Infrastructure.Persistence.Configurations;

/// <summary>Coleção "file_lines". Os valores da linha ficam como subdocumento (coluna → valor).</summary>
internal static class FileLineConfiguration
{
    public static void Configure() =>
        BsonClassMap.TryRegisterClassMap<FileLine>(map =>
        {
            map.AutoMap();
            map.UnmapProperty(l => l.IsPending);
            map.MapProperty(l => l.Values).SetSerializer(
                new ImpliedImplementationInterfaceSerializer<IReadOnlyDictionary<string, string>, Dictionary<string, string>>(
                    new DictionaryInterfaceImplementerSerializer<Dictionary<string, string>>(DictionaryRepresentation.Document)));
        });
}
