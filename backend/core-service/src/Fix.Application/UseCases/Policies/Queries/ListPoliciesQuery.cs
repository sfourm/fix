using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Policies.Dtos;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Policies.Queries;

[RequireRole(RoleCodes.ViewPolicy)]
public sealed record ListPoliciesQuery(
    Guid UserId,
    Guid OrganizationId,
    int Page,
    int PageSize)
    : IQuery<PagedList<PolicySummaryDto>>, IOrganizationRequest;

