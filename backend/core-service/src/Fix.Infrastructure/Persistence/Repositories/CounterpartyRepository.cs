using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Counterparties.Repositories;
using Fix.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

/// <summary>Isolamento por tenant garantido pelo query filter do <see cref="FixDbContext"/>.</summary>
internal sealed class CounterpartyRepository(FixDbContext dbContext) : ICounterpartyRepository
{
    public Task<Counterparty?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Counterparties.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Counterparty>> ListAsync(bool onlyHomologated, CancellationToken cancellationToken)
    {
        var query = dbContext.Counterparties.AsNoTracking();
        if (onlyHomologated)
        {
            query = query.Where(c => c.IsHomologated);
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        var value = Name.Create(name);
        return dbContext.Counterparties.AnyAsync(c => c.Name == value && c.Id != exceptId, cancellationToken);
    }

    public Task<int> CountUsagesAsync(Guid counterpartyId, CancellationToken cancellationToken) =>
        dbContext.Orders.CountAsync(o => o.CounterpartyId == counterpartyId, cancellationToken);

    // O filtro de organização do DbContext limita ao tenant atual.
    public async Task<int> NextNumberAsync(CancellationToken cancellationToken) =>
        (await dbContext.Counterparties.MaxAsync(c => (int?)c.Number, cancellationToken) ?? 0) + 1;

    public void Add(Counterparty counterparty) => dbContext.Counterparties.Add(counterparty);

    public void Remove(Counterparty counterparty) => dbContext.Counterparties.Remove(counterparty);
}

