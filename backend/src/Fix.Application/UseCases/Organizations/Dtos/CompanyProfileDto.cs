using Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Application.Organizations.Dtos;

public sealed record CompanyProfileDto(
    string CorporateName,
    string? TaxId,
    string? Headquarters,
    string? Group,
    Sector Sector,
    int CropYearStartMonth,
    string? ActiveCrop);

