using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Orders;

public sealed class OrderTimeline : Timeline
{
    private OrderTimeline()
    {
    }

    public OrderTimeline(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt)
        : base(organizationId, entityId, nameof(Order), action, snapshot, authorId, occurredAt)
    {
    }
}
