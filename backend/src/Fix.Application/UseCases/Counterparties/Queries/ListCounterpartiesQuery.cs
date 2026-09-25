using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Counterparties.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Counterparties.Queries;

[RequireRole(RoleCodes.ViewCounterparties)]
public sealed record ListCounterpartiesQuery(Guid UserId, Guid OrganizationId, bool OnlyHomologated)
    : IQuery<IReadOnlyList<CounterpartyDto>>, IOrganizationRequest;

