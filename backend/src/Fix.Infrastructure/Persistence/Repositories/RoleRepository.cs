using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Roles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository(FixDbContext dbContext) : IRoleRepository
{
    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Set<Role>().AsNoTracking().OrderBy(r => r.Code).ToListAsync(cancellationToken);
}

