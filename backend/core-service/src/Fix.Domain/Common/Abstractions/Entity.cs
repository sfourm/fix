namespace Fix.Domain.Abstractions;

/// <summary>
/// Base de todas as entidades. A auditoria (created_at, updated_at, author_created, author_updated)
/// é aplicada pela infraestrutura via shadow properties, sem poluir o domínio.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    protected Entity() => Id = Guid.CreateVersion7();

    protected Entity(Guid id) => Id = id;

    public Guid Id { get; private init; }

    public bool Equals(Entity? other) =>
        other is not null && other.GetType() == GetType() && other.Id == Id;

    public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
