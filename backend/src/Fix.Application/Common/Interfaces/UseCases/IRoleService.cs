using Fix.Application.Roles.Dtos;
using Fix.Application.Roles.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Roles: permissões atômicas do sistema. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> ListRolesAsync(ListRolesQuery query, CancellationToken cancellationToken);
}
