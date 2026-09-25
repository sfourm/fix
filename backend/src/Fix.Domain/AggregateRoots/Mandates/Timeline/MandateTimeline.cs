using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Mandates;

public sealed class MandateTimeline : Timeline
{
    private MandateTimeline()
    {
    }

    public MandateTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(Mandate), action, snapshot, authorId, occurredAt)
    {
    }
}
