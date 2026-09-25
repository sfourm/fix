using Fix.Application.Abstractions.Messaging;
using Fix.Application.Roles.Dtos;

namespace Fix.Application.Roles.Queries;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleDto>>;
