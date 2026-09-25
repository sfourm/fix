using Fix.Domain.Abstractions;
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

        return query
            .OrderByDescending(o => o.Id)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public void Add(Order order) => dbContext.Orders.Add(order);

    public void Remove(Order order) => dbContext.Orders.Remove(order);
}

