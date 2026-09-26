using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Rules.Dtos;

namespace Fix.Application.Rules.Commands;

/// <summary>Cria uma alçada personalizada da organização com as roles escolhidas.</summary>
[RequireRole(global::Fix.Domain.AggregateRoots.Roles.RoleCodes.UpdateUser)]
public sealed record CreateRuleCommand(Guid UserId, Guid OrganizationId, string Name, IReadOnlyList<string> RoleCodes)
    : ICommand<RuleDto>, IOrganizationRequest;
