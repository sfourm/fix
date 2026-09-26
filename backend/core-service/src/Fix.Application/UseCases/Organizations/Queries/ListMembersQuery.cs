using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Queries;

[RequireRole(RoleCodes.ViewUser)]
public sealed record ListMembersQuery(Guid UserId, Guid OrganizationId)
    : IQuery<IReadOnlyList<MemberDto>>, IOrganizationRequest;

