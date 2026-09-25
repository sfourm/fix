using Fix.Application.Organizations.Dtos;
using Contract = Fix.Contracts.V1;

namespace Fix.Presentation.Mappers;

internal static class OrganizationContractMapper
{
    public static Contract.Organization ToContract(this OrganizationDto organization) => new()
    {
        Id = organization.Id.ToString(),
        Name = organization.Name,
        Slug = organization.Slug,
    };

    public static Contract.OrganizationSetup ToContract(this OrganizationSetupDto setup) => new()
    {
        Id = setup.Id.ToString(),
        Name = setup.Name,
        Slug = setup.Slug,
        Profile = setup.Profile.ToContract(),
        Industrial = setup.Industrial.ToContract(),
        Budget = setup.Budget.ToContract(),
        Financials = setup.Financials.ToContract(),
        Commodities = { setup.Commodities.Select(c => c.ToContract()) },
    };

    public static Contract.Member ToContract(this MemberDto member) => new()
    {
        Id = member.Id.ToString(),
        UserId = member.UserId.ToString(),
        Email = member.Email,
        FullName = member.FullName,
        Desk = member.Desk.ToContract<Contract.Desk>(),
        Rules = { member.Rules },
        Groups = { member.Groups },
    };

    public static Contract.Group ToContract(this GroupDto group) => new()
    {
        Id = group.Id.ToString(),
        Name = group.Name,
        IsDefault = group.IsDefault,
        Rules = { group.Rules },
        MemberIds = { group.MemberIds.Select(id => id.ToString()) },
    };

    private static Contract.CompanyProfile ToContract(this CompanyProfileDto profile)
    {
        var contract = new Contract.CompanyProfile
        {
            CorporateName = profile.CorporateName,
            Sector = profile.Sector.ToContract<Contract.Sector>(),
            CropYearStartMonth = profile.CropYearStartMonth,
        };

        if (profile.TaxId is { } taxId) contract.TaxId = taxId;
        if (profile.Headquarters is { } headquarters) contract.Headquarters = headquarters;
        if (profile.Group is { } group) contract.Group = group;
        if (profile.ActiveCrop is { } crop) contract.ActiveCrop = crop;
        return contract;
    }

    private static Contract.IndustrialProfile ToContract(this IndustrialProfileDto industrial)
    {
        var contract = new Contract.IndustrialProfile();
        if (industrial.MillingCapacity is { } milling) contract.MillingCapacity = (double)milling;
        if (industrial.MixMinPct is { } min) contract.MixMinPct = (double)min;
        if (industrial.MixMaxPct is { } max) contract.MixMaxPct = (double)max;
        if (industrial.MixGuidancePct is { } guidance) contract.MixGuidancePct = (double)guidance;
        return contract;
    }

    private static Contract.Budget ToContract(this BudgetDto budget)
    {
        var contract = new Contract.Budget();
        if (budget.CashCost is { } cashCost) contract.CashCost = (double)cashCost;
        if (budget.EconomicFloor is { } floor) contract.EconomicFloor = (double)floor;
        if (budget.EquivalentPrice is { } equivalent) contract.EquivalentPrice = (double)equivalent;
        if (budget.TargetMarginPct is { } margin) contract.TargetMarginPct = (double)margin;
        return contract;
    }

    private static Contract.Financials ToContract(this FinancialsDto financials)
    {
        var contract = new Contract.Financials();
        if (financials.Cash is { } cash) contract.Cash = (double)cash;
        if (financials.CreditLines is { } lines) contract.CreditLines = (double)lines;
        if (financials.MonthlyFixedCost is { } fixedCost) contract.MonthlyFixedCost = (double)fixedCost;
        if (financials.NetDebt is { } netDebt) contract.NetDebt = (double)netDebt;
        if (financials.Ebitda is { } ebitda) contract.Ebitda = (double)ebitda;
        if (financials.UsdDebt is { } usdDebt) contract.UsdDebt = (double)usdDebt;
        if (financials.ReferenceDate is { } date) contract.ReferenceDate = date.ToContract();
        if (financials.Leverage is { } leverage) contract.Leverage = (double)leverage;
        return contract;
    }

    private static Contract.OrganizationCommodity ToContract(this CommodityDto commodity)
    {
        var contract = new Contract.OrganizationCommodity
        {
            Id = commodity.Id.ToString(),
            Commodity = commodity.Commodity.ToContract<Contract.Commodity>(),
            Capacity = (double)commodity.Capacity,
            Unit = commodity.Unit.ToContract<Contract.MeasurementUnit>(),
            Currency = commodity.Currency,
            Sells = commodity.Sells,
        };

        if (commodity.PriceReference is { } reference) contract.PriceReference = reference;
        return contract;
    }
}
