using Fix.Domain.AggregateRoots.Rules;
using Fix.Domain.AggregateRoots.Rules.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

internal sealed class RuleRepository(FixDbContext dbContext) : IRuleRepository
{
    public async Task<IReadOnlyList<Rule>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Rules
            .AsNoTracking()
            .Include(r => r.Roles)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

    public Task<Rule?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Rules
            .AsNoTracking()
            .Include(r => r.Roles)
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
}

