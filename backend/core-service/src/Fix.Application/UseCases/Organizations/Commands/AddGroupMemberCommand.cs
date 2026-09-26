using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.UpdateUser)]
public sealed record AddGroupMemberCommand(Guid UserId, Guid OrganizationId, Guid GroupId, Guid MemberId)
    : ICommand, IOrganizationRequest;

