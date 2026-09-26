using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Fix.Storage.Infrastructure.Persistence.Configurations;

/// <summary>
/// Convenções do banco (uma vez por processo): campos em camelCase, enums como texto, Guid no formato padrão (UUID)
/// e datas como Date em UTC. Depois, o mapeamento de cada entidade.
/// </summary>
internal static class MongoConfiguration
{
    private static readonly Lock Gate = new();
    private static bool applied;

    public static void Apply()
    {
        lock (Gate)
        {
            if (applied)
            {
                return;
            }

            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.TryRegisterSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            ConventionRegistry.Register(
                "fix-storage",
                new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new EnumRepresentationConvention(BsonType.String),
                    new IgnoreExtraElementsConvention(true),
                },
                type => type.Namespace?.StartsWith("Fix.Storage", StringComparison.Ordinal) == true);

            EntityConfiguration.Configure();
            FileConfiguration.Configure();
            FileLineConfiguration.Configure();
            applied = true;
        }
    }
}
