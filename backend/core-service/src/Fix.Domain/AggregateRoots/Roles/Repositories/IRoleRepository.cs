using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Domain.AggregateRoots.Roles.Repositories;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken);
}
