using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Domain.AggregateRoots.Orders.Repositories;

/// <summary>Filtros da lista de boletas. WithoutMandate e OnlyOutside montam a lista de exceções (desvios expostos).</summary>
public sealed record OrderFilter(
    Guid? MandateId,
    ApprovalStatus? Approval,
    ConfirmationStatus? Confirmation,
    bool WithoutMandate = false,
    bool OnlyOutside = false);

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<Order>> ListAsync(OrderFilter filter, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Próximo número sequencial de boleta da organização (HX-0001...).</summary>
    Task<int> NextNumberAsync(CancellationToken cancellationToken);

    void Add(Order order);

    void Remove(Order order);
}
