using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Queries;

[RequireRole(RoleCodes.ViewOrder)]
public sealed record GetOrderQuery(Guid UserId, Guid OrganizationId, Guid Id)
    : IQuery<OrderDto>, IOrganizationRequest;

