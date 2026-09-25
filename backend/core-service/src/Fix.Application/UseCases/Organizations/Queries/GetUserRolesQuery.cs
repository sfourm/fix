using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;

namespace Fix.Application.Organizations.Queries;

/// <summary>Roles (alçadas) efetivas do usuário na organização — o BFF/front usa para esconder ou mostrar ações.</summary>
[RequireMembership]
public sealed record GetUserRolesQuery(Guid UserId, Guid OrganizationId)
    : IQuery<IReadOnlyCollection<string>>, IOrganizationRequest;
