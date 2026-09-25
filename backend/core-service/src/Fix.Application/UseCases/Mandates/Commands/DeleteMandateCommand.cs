using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Mandates.Commands;

[RequireRole(RoleCodes.DeleteMandate)]
public sealed record DeleteMandateCommand(Guid UserId, Guid OrganizationId, Guid Id)
    : ICommand, IOrganizationRequest;

