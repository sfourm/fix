using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Commands;

[RequireRole(RoleCodes.UpdateOrder)]
public sealed record UpdateOrderCommand(Guid UserId, Guid OrganizationId, Guid Id, Guid CounterpartyId, OrderTermsInput Terms)
    : ICommand<OrderDto>, IOrganizationRequest;

