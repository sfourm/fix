using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Substitui as alçadas do grupo (todos os membros do grupo recebem as roles delas).</summary>
[RequireRole(RoleCodes.UpdateUser)]
public sealed record SetGroupAlcadasCommand(Guid UserId, Guid OrganizationId, Guid GroupId, IReadOnlyList<string> RuleCodes)
    : ICommand, IOrganizationRequest;
