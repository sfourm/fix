using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Policies.Commands;

[RequireRole(RoleCodes.DeletePolicy)]
public sealed record DeletePolicyCommand(Guid UserId, Guid OrganizationId, Guid Id)
    : ICommand, IOrganizationRequest;

