using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record ChangeMemberDeskCommand(Guid UserId, Guid OrganizationId, Guid MemberId, Desk? Desk)
    : ICommand, IOrganizationRequest;

