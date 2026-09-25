using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Counterparties.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Counterparties.Commands;

[RequireRole(RoleCodes.ManageCounterparties)]
public sealed record SetCounterpartyHomologationCommand(Guid UserId, Guid OrganizationId, Guid Id, bool Homologated)
    : ICommand<CounterpartyDto>, IOrganizationRequest;

