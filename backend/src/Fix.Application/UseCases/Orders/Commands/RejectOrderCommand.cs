using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Commands;

[RequireRole(RoleCodes.ApproveOrder)]
public sealed record RejectOrderCommand(Guid UserId, Guid OrganizationId, Guid Id, string Reason)
    : ICommand<OrderDto>, IOrganizationRequest;

