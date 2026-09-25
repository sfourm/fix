using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Commands;

[RequireRole(RoleCodes.DeleteOrder)]
public sealed record DeleteOrderCommand(Guid UserId, Guid OrganizationId, Guid Id)
    : ICommand, IOrganizationRequest;

