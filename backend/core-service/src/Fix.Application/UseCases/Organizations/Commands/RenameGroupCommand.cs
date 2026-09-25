using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Renomeia um grupo do organograma.</summary>
[RequireRole(RoleCodes.EditOrganization)]
public sealed record RenameGroupCommand(Guid UserId, Guid OrganizationId, Guid GroupId, string Name)
    : ICommand, IOrganizationRequest;
