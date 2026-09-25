using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record UpdateIndustrialProfileCommand(
    Guid UserId,
    Guid OrganizationId,
    decimal? MillingCapacity,
    decimal? MixMinPct,
    decimal? MixMaxPct,
    decimal? MixGuidancePct)
    : ICommand<OrganizationSetupDto>, IOrganizationRequest;

