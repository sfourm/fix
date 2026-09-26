using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Move o grupo (com os grupos abaixo dele) para baixo de outro grupo no organograma.</summary>
[RequireRole(RoleCodes.UpdateUser)]
public sealed record MoveGroupCommand(Guid UserId, Guid OrganizationId, Guid GroupId, Guid ParentGroupId)
    : ICommand, IOrganizationRequest;
