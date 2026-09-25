using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.AggregateRoots.Policies.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

/// <summary>Isolamento por tenant garantido pelo query filter do <see cref="FixDbContext"/>.</summary>
internal sealed class PolicyRepository(FixDbContext dbContext) : IPolicyRepository
{
    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        WithChildren(dbContext.Policies).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<PagedList<Policy>> ListAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        dbContext.Policies
            .AsNoTracking()
            .Include(p => p.Axes)
            .OrderByDescending(p => p.Id)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public async Task<IReadOnlyList<Policy>> ListActiveAsync(CancellationToken cancellationToken) =>
        await WithChildren(dbContext.Policies).Where(p => p.Status == PolicyStatus.Active).ToListAsync(cancellationToken);

    public Task<bool> CodeExistsAsync(string code, Guid? exceptId, CancellationToken cancellationToken) =>
        dbContext.Policies.AnyAsync(p => p.Code == code && p.Id != exceptId, cancellationToken);

    public Task<bool> HasMandatesAsync(Guid policyId, CancellationToken cancellationToken) =>
        dbContext.Mandates.AnyAsync(m => m.PolicyId == policyId, cancellationToken);

    public Task<bool> AxisHasMandatesAsync(Guid axisId, CancellationToken cancellationToken) =>
        dbContext.Mandates.AnyAsync(m => m.AxisId == axisId, cancellationToken);

    public void Add(Policy policy) => dbContext.Policies.Add(policy);

    public void Remove(Policy policy) => dbContext.Policies.Remove(policy);

    private static IQueryable<Policy> WithChildren(IQueryable<Policy> query) =>
        query
            .Include(p => p.Axes)
            .Include(p => p.Bands)
            .Include(p => p.Instruments)
            .Include(p => p.Versions)
            .AsSplitQuery();
}

