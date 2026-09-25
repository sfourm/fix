using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

public sealed class OrganizationTimeline : Timeline
{
    private OrganizationTimeline()
    {
    }

    public OrganizationTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(Organization), action, snapshot, authorId, occurredAt)
    {
    }
}
