using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Policies.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Policies.Queries;

[RequireRole(RoleCodes.ViewPolicy)]
public sealed record GetPolicyQuery(Guid UserId, Guid OrganizationId, Guid Id)
    : IQuery<PolicyDto>, IOrganizationRequest;

