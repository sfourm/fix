using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record UpdateCompanyProfileCommand(
    Guid UserId,
    Guid OrganizationId,
    string CorporateName,
    string? TaxId,
    string? Headquarters,
    string? Group,
    Sector Sector,
    int CropYearStartMonth,
    string? ActiveCrop)
    : ICommand<OrganizationSetupDto>, IOrganizationRequest;

