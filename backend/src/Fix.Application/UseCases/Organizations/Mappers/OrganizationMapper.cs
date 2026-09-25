using Fix.Application.Abstractions.Authentication;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Application.Organizations.Mappers;

internal static class OrganizationMapper
{
    public static OrganizationDto ToDto(this Organization organization) =>
        new(organization.Id, organization.Name.Value, organization.Slug.Value);

    public static OrganizationSetupDto ToSetupDto(this Organization organization) => new(
        organization.Id,
        organization.Name.Value,
        organization.Slug.Value,
        new CompanyProfileDto(
            organization.Profile.CorporateName,
            organization.Profile.TaxId,
            organization.Profile.Headquarters,
            organization.Profile.Group,
            organization.Profile.Sector,
            organization.Profile.CropYearStartMonth,
            organization.Profile.ActiveCrop),
        new IndustrialProfileDto(
            organization.Industrial.MillingCapacity,
            organization.Industrial.MixMinPct,
            organization.Industrial.MixMaxPct,
            organization.Industrial.MixGuidancePct),
        new BudgetDto(
            organization.Budget.CashCost,
            organization.Budget.EconomicFloor,
            organization.Budget.EquivalentPrice,
            organization.Budget.TargetMarginPct),
        new FinancialsDto(
            organization.Financials.Cash,
            organization.Financials.CreditLines,
            organization.Financials.MonthlyFixedCost,
            organization.Financials.NetDebt,
            organization.Financials.Ebitda,
            organization.Financials.UsdDebt,
            organization.Financials.ReferenceDate,
            organization.Financials.Leverage),
        organization.Commodities
            .OrderBy(c => c.Commodity)
            .Select(c => new CommodityDto(c.Id, c.Commodity, c.Capacity, c.Unit, c.PriceReference, c.Currency, c.Sells))
            .ToList());

    public static IReadOnlyList<MemberDto> ToMemberDtos(
        this Organization organization,
        IReadOnlyDictionary<Guid, UserInfo> users,
        IReadOnlyDictionary<Guid, string> ruleCodes) =>
        organization.Members
            .Select(member =>
            {
                users.TryGetValue(member.UserId, out var user);

                var rules = organization.Rules
                    .Where(r => r.MemberId == member.Id)
                    .Select(r => ruleCodes.GetValueOrDefault(r.RuleId, r.RuleId.ToString()))
                    .ToList();

                var groups = organization.Groups
                    .Where(g => g.Members.Any(gm => gm.MemberId == member.Id))
                    .Select(g => g.Name.Value)
                    .ToList();

                return new MemberDto(
                    member.Id,
                    member.UserId,
                    user?.Email ?? string.Empty,
                    user?.FullName ?? string.Empty,
                    member.Desk,
                    rules,
                    groups);
            })
            .ToList();

    public static IReadOnlyList<GroupDto> ToGroupDtos(
        this Organization organization,
        IReadOnlyDictionary<Guid, string> ruleCodes) =>
        organization.Groups
            .Select(group => new GroupDto(
                group.Id,
                group.Name.Value,
                group.IsDefault,
                organization.Rules
                    .Where(r => r.GroupId == group.Id)
                    .Select(r => ruleCodes.GetValueOrDefault(r.RuleId, r.RuleId.ToString()))
                    .ToList(),
                group.Members.Select(m => m.MemberId).ToList()))
            .ToList();
}

