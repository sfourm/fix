using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Mandates.Repositories;
using Fix.Domain.AggregateRoots.Orders;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

internal sealed class MandateRepository(FixDbContext dbContext) : IMandateRepository
{
    public Task<Mandate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Mandates.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<PagedList<Mandate>> ListAsync(MandateFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Mandates.AsNoTracking();
        if (filter.PolicyId is { } policyId)
        {
            query = query.Where(m => m.PolicyId == policyId);
        }

        if (filter.Status is { } status)
        {
            query = query.Where(m => m.Status == status);
        }

        return query.OrderByDescending(m => m.Id).ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<bool> HasOrdersAsync(Guid mandateId, CancellationToken cancellationToken) =>
        dbContext.Orders.AnyAsync(o => o.MandateId == mandateId, cancellationToken);

    /// <summary>Soma lotes (futuros/opções) e nocional (NDF) das boletas aprovadas.</summary>
    public async Task<IReadOnlyDictionary<Guid, decimal>> GetConsumedAsync(
        IReadOnlyCollection<Guid> mandateIds,
        Guid? exceptOrderId,
        CancellationToken cancellationToken)
    {
        if (mandateIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var rows = await dbContext.Orders
            .AsNoTracking()
            .Where(o => mandateIds.Contains(o.MandateId)
                && o.Approval == ApprovalStatus.Approved
                && o.Id != exceptOrderId)
            .GroupBy(o => o.MandateId)
            .Select(g => new { MandateId = g.Key, Total = g.Sum(o => (o.Lots ?? 0) + (o.NotionalUsd ?? 0)) })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.MandateId, r => r.Total);
    }

    public void Add(Mandate mandate) => dbContext.Mandates.Add(mandate);

    public void Remove(Mandate mandate) => dbContext.Mandates.Remove(mandate);
}

