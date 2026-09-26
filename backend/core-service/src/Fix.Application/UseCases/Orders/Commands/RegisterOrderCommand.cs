using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Commands;

[RequireRole(RoleCodes.CreateOrder)]
public sealed record RegisterOrderCommand(
    Guid UserId,
    Guid OrganizationId,
    /// <summary>Nulo = boleta sem mandato (desvio: exige justificativa e fica exposto).</summary>
    Guid? MandateId,
    Guid CounterpartyId,
    OrderTermsInput Terms)
    : ICommand<OrderDto>, IOrganizationRequest;

