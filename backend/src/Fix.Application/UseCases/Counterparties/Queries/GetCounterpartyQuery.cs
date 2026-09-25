using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Counterparties.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Counterparties.Queries;

[RequireRole(RoleCodes.ViewCounterparties)]
public sealed record GetCounterpartyQuery(Guid UserId, Guid OrganizationId, Guid Id)
    : IQuery<CounterpartyDto>, IOrganizationRequest;

