using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

public sealed class OrganizationGroupTimeline : Timeline
{
    private OrganizationGroupTimeline()
    {
    }

    public OrganizationGroupTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(OrganizationGroup), action, snapshot, authorId, occurredAt)
    {
    }
}
