using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Rules;

public sealed class RuleTimeline : Timeline
{
    private RuleTimeline()
    {
    }

    public RuleTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(Rule), action, snapshot, authorId, occurredAt)
    {
    }
}
