using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;

namespace Fix.Application.Organizations.Queries;

/// <summary>Organizações das quais o usuário é membro (não exige tenant).</summary>
public sealed record ListUserOrganizationsQuery(Guid UserId) : IQuery<IReadOnlyList<OrganizationDto>>, IUserRequest;
