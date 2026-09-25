using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Rules.Dtos;

namespace Fix.Application.Rules.Commands;

/// <summary>Renomeia a alçada e troca as roles que ela concede (vale na hora para todos que a têm).</summary>
[RequireRole(global::Fix.Domain.AggregateRoots.Roles.RoleCodes.EditOrganization)]
public sealed record UpdateRuleCommand(Guid UserId, Guid OrganizationId, Guid Id, string Name, IReadOnlyList<string> RoleCodes)
    : ICommand<RuleDto>, IOrganizationRequest;
