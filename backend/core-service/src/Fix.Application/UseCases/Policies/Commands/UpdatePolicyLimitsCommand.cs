using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Policies.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Policies.Commands;

[RequireRole(RoleCodes.UpdatePolicy)]
public sealed record UpdatePolicyLimitsCommand(Guid UserId, Guid OrganizationId, Guid Id, PolicyLimitsDto Limits)
    : ICommand<PolicyDto>, IOrganizationRequest;

