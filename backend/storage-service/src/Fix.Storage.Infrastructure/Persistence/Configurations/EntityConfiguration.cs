using MongoDB.Bson.Serialization;

namespace Fix.Storage.Infrastructure.Persistence.Configurations;

/// <summary>Base das entidades: o Id vira o _id do documento.</summary>
internal static class EntityConfiguration
{
    public static void Configure() =>
        BsonClassMap.TryRegisterClassMap<Entity>(map =>
        {
            map.SetIsRootClass(false);
            map.MapIdProperty(e => e.Id);
        });
}
