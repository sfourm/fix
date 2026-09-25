using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record AddCommodityCommand(
    Guid UserId,
    Guid OrganizationId,
    Commodity Commodity,
    decimal Capacity,
    MeasurementUnit Unit,
    string? PriceReference,
    string Currency,
    bool Sells)
    : ICommand<OrganizationSetupDto>, IOrganizationRequest;

