using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record UpdateFinancialsCommand(
    Guid UserId,
    Guid OrganizationId,
    decimal? Cash,
    decimal? CreditLines,
    decimal? MonthlyFixedCost,
    decimal? NetDebt,
    decimal? Ebitda,
    decimal? UsdDebt,
    DateOnly? ReferenceDate)
    : ICommand<OrganizationSetupDto>, IOrganizationRequest;

