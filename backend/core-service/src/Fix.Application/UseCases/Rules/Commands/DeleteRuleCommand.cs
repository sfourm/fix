using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Rules.Commands;

/// <summary>Exclui a alçada, retirando-a de todos os membros e grupos que a tinham.</summary>
[RequireRole(RoleCodes.EditOrganization)]
public sealed record DeleteRuleCommand(Guid UserId, Guid OrganizationId, Guid Id)
    : ICommand, IOrganizationRequest;
