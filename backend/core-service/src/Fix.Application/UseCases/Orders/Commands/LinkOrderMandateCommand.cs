using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Commands;

/// <summary>Vínculo a posteriori: liga uma boleta sem mandato a um mandato ativo, com justificativa (carimbo permanente).</summary>
[RequireRole(RoleCodes.UpdateOrder)]
public sealed record LinkOrderMandateCommand(Guid UserId, Guid OrganizationId, Guid Id, Guid MandateId, string Justification)
    : ICommand<OrderDto>, IOrganizationRequest;
