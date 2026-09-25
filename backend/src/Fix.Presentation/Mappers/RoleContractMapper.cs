using Fix.Application.Roles.Dtos;
using Fix.Contracts.V1;

namespace Fix.Presentation.Mappers;

internal static class RoleContractMapper
{
    public static Role ToContract(this RoleDto role) => new()
    {
        Id = role.Id.ToString(),
        Code = role.Code,
        Description = role.Description,
    };
}
