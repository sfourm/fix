namespace Fix.Application.Organizations.Dtos;

/// <summary>Setup completo da companhia (tela Setup).</summary>
public sealed record OrganizationSetupDto(
    Guid Id,
    string Name,
    string Slug,
    CompanyProfileDto Profile,
    IndustrialProfileDto Industrial,
    BudgetDto Budget,
    FinancialsDto Financials,
    IReadOnlyList<CommodityDto> Commodities);
