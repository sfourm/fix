using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Counterparties;

public sealed class CounterpartyTimeline : Timeline
{
    private CounterpartyTimeline()
    {
    }

    public CounterpartyTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(Counterparty), action, snapshot, authorId, occurredAt)
    {
    }
}
