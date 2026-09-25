using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Roles.Dtos;
using Fix.Application.Roles.Mappers;
using Fix.Application.Roles.Queries;
using Fix.Domain.AggregateRoots.Roles.Repositories;

namespace Fix.Application.Roles.Services;

internal sealed class RoleService(IRoleRepository roleRepository)
    : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(ListRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.ListAsync(cancellationToken);
        return roles.Select(r => r.ToDto()).ToList();
    }
}
