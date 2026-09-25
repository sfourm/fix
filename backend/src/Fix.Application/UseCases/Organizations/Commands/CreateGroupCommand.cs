using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record CreateGroupCommand(Guid UserId, Guid OrganizationId, string Name, IReadOnlyList<string> RuleCodes)
    : ICommand<Guid>, IOrganizationRequest;

