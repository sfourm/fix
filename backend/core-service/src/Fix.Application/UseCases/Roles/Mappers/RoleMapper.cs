using Fix.Application.Roles.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Roles.Mappers;

internal static class RoleMapper
{
    public static RoleDto ToDto(this Role role) => new(role.Id, role.Code, role.Description);
}

