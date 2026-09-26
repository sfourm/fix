using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Orders.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(FixDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<PagedList<Order>> ListAsync(OrderFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking();
        if (filter.MandateId is { } mandateId)
        {
            query = query.Where(o => o.MandateId == mandateId);
        }

        if (filter.Approval is { } approval)
        {
            query = query.Where(o => o.Approval == approval);
        }

        if (filter.Confirmation is { } confirmation)
        {
            query = query.Where(o => o.Confirmation == confirmation);
        }

        if (filter.WithoutMandate)
        {
            query = query.Where(o => o.MandateId == null);
        }

        if (filter.OnlyOutside)
        {
            query = query.Where(o => o.Compliance.Status == ComplianceStatus.Outside);
        }

        return query
            .OrderByDescending(o => o.Id)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    // O filtro de organização do DbContext limita ao tenant atual.
    public async Task<int> NextNumberAsync(CancellationToken cancellationToken) =>
        (await dbContext.Orders.MaxAsync(o => (int?)o.Number, cancellationToken) ?? 0) + 1;

    public void Add(Order order) => dbContext.Orders.Add(order);

    public void Remove(Order order) => dbContext.Orders.Remove(order);
}

