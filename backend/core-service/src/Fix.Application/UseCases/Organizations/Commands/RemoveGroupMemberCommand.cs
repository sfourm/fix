using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Tira o membro do grupo (ele continua na organização).</summary>
[RequireRole(RoleCodes.EditOrganization)]
public sealed record RemoveGroupMemberCommand(Guid UserId, Guid OrganizationId, Guid GroupId, Guid MemberId)
    : ICommand, IOrganizationRequest;
