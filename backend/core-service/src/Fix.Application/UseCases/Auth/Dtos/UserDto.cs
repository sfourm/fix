namespace Fix.Application.Auth.Dtos;

public sealed record UserDto(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);
