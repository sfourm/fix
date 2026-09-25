using Fix.Domain.AggregateRoots.Rules;
using Fix.Domain.AggregateRoots.Rules.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

internal sealed class RuleRepository(FixDbContext dbContext) : IRuleRepository
{
    public async Task<IReadOnlyList<Rule>> ListForOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await dbContext.Rules
            .AsNoTracking()
            .Include(r => r.Roles)
            .Where(r => r.OrganizationId == null || r.OrganizationId == organizationId)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

    public Task<Rule?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Rules.Include(r => r.Roles).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Rule>> ListAlcadasByCodeAsync(
        Guid organizationId,
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken) =>
        await dbContext.Rules
            .Where(r => r.OrganizationId == organizationId && codes.Contains(r.Code))
            .ToListAsync(cancellationToken);

    public Task<bool> CodeExistsAsync(Guid organizationId, string code, Guid? exceptId, CancellationToken cancellationToken) =>
        dbContext.Rules.AnyAsync(r => r.OrganizationId == organizationId && r.Code == code && r.Id != exceptId, cancellationToken);

    public void Add(Rule rule) => dbContext.Rules.Add(rule);

    public void Remove(Rule rule) => dbContext.Rules.Remove(rule);
}
