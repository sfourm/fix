using Fix.Application.Auth.Dtos;
using Fix.Contracts.V1;

namespace Fix.Presentation.Mappers;

internal static class AuthContractMapper
{
    public static User ToContract(this UserDto user) => new()
    {
        Id = user.Id.ToString(),
        Email = user.Email,
        FullName = user.FullName,
        Roles = { user.Roles },
    };
}
