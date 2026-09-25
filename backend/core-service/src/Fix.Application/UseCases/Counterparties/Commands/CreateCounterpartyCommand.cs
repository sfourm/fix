using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Counterparties.Dtos;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Counterparties.Commands;

[RequireRole(RoleCodes.ManageCounterparties)]
public sealed record CreateCounterpartyCommand(
    Guid UserId,
    Guid OrganizationId,
    string Name,
    CounterpartyType Type,
    string? Document,
    string? Address,
    string? Country,
    decimal? NotionalLimitUsd,
    decimal? MtmLimitUsd)
    : ICommand<CounterpartyDto>, IOrganizationRequest;

