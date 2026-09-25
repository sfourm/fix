namespace Fix.Application.Roles.Dtos;

/// <summary>Role: permissão atômica (ex.: create_policy).</summary>
public sealed record RoleDto(Guid Id, string Code, string Description);
