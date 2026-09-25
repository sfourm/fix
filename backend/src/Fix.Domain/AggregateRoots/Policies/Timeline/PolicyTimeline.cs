using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Policies;

public sealed class PolicyTimeline : Timeline
{
    private PolicyTimeline()
    {
    }

    public PolicyTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(Policy), action, snapshot, authorId, occurredAt)
    {
    }
}
