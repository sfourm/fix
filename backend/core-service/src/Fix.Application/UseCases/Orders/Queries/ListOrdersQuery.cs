using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Dtos;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Queries;

[RequireRole(RoleCodes.ViewOrder)]
public sealed record ListOrdersQuery(
    Guid UserId,
    Guid OrganizationId,
    Guid? MandateId,
    ApprovalStatus? Approval,
    ConfirmationStatus? Confirmation,
    int Page,
    int PageSize,
    bool WithoutMandate = false,
    bool OnlyOutside = false)
    : IQuery<PagedList<OrderDto>>, IOrganizationRequest;

