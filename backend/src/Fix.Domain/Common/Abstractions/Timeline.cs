namespace Fix.Domain.Abstractions;

public abstract class Timeline : Entity
{
    protected Timeline()
    {
    }

    protected Timeline(
        Guid? organizationId,
        Guid entityId,
        string entityType,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
    {
        OrganizationId = organizationId;
        EntityId = entityId;
        EntityType = entityType;
        Action = action;
        Snapshot = snapshot;
        AuthorId = authorId;
        OccurredAt = occurredAt;
    }

    public Guid? OrganizationId { get; private init; }

    public string EntityType { get; private init; } = string.Empty;

    public Guid EntityId { get; private init; }

    public string Action { get; private init; } = string.Empty;

    public string Snapshot { get; private init; } = string.Empty;

    public Guid? AuthorId { get; private init; }

    public DateTimeOffset OccurredAt { get; private init; }
}
