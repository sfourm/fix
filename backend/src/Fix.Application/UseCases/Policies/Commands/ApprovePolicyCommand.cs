using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Policies.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Policies.Commands;

[RequireRole(RoleCodes.ApprovePolicy)]
public sealed record ApprovePolicyCommand(Guid UserId, Guid OrganizationId, Guid Id, string ApprovalRecord)
    : ICommand<PolicyDto>, IOrganizationRequest;

