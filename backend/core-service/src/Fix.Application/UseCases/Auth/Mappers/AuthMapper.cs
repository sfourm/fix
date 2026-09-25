using Fix.Application.Abstractions.Authentication;
using Fix.Application.Auth.Dtos;

namespace Fix.Application.Auth.Mappers;

internal static class AuthMapper
{
    public static UserDto ToDto(this UserInfo user) => new(user.Id, user.Email, user.FullName, user.Roles);
}
