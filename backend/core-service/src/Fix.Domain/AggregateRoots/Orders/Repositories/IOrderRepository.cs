using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Domain.AggregateRoots.Orders.Repositories;

public sealed record OrderFilter(Guid? MandateId, ApprovalStatus? Approval, ConfirmationStatus? Confirmation);

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<Order>> ListAsync(OrderFilter filter, int page, int pageSize, CancellationToken cancellationToken);

    void Add(Order order);

    void Remove(Order order);
}
