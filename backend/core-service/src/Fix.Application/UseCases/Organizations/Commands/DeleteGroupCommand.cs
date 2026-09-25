using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Exclui um grupo do organograma (e as alçadas atribuídas a ele).</summary>
[RequireRole(RoleCodes.EditOrganization)]
public sealed record DeleteGroupCommand(Guid UserId, Guid OrganizationId, Guid GroupId)
    : ICommand, IOrganizationRequest;
