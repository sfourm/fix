using Fix.Application.Policies.Dtos;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Mappers;

internal static class PolicyMapper
{
    public static PolicySummaryDto ToSummaryDto(this Policy policy) => new(
        policy.Id,
        policy.Code,
        policy.Title.Value,
        policy.Version,
        policy.Status,
        policy.Validity.StartsOn,
        policy.Validity.EndsOn,
        policy.Axes.Count);

    public static PolicyDto ToDto(this Policy policy) => new(
        policy.Id,
        policy.Code,
        policy.Title.Value,
        policy.Version,
        policy.Description,
        policy.Status,
        policy.Validity.StartsOn,
        policy.Validity.EndsOn,
        policy.ApprovalRecord,
        policy.ApprovedOn,
        policy.Limits.ToDto(),
        policy.Axes
            .OrderBy(a => a.Code)
            .Select(a => new PolicyAxisDto(
                a.Id,
                a.Code,
                a.Title.Value,
                a.Factor,
                a.Statement,
                a.LimitDescription,
                a.Approver,
                a.Restrictions.ToList()))
            .ToList(),
        policy.Bands
            .OrderBy(b => b.Crop.StartYear)
            .Select(b => new CoverageBandDto(b.Id, b.Horizon, b.Crop.Value, b.MinPct, b.MaxPct, b.Note))
            .ToList(),
        policy.Instruments
            .OrderBy(i => i.Permission)
            .ThenBy(i => i.Name)
            .Select(i => new PolicyInstrumentDto(i.Id, i.Name, i.Permission, i.Condition))
            .ToList(),
        policy.Versions
            .OrderBy(v => v.Date)
            .ThenBy(v => v.Id)
            .Select(v => new PolicyVersionDto(v.Version, v.Status, v.Date, v.Note))
            .ToList());

    public static PolicyLimitsDto ToDto(this PolicyLimits limits) => new(
        limits.HedgeHorizonYears,
        limits.AbsoluteCeilingPct,
        limits.FxFixedMinPct,
        limits.FxFixedMaxPct,
        limits.FxUnfixedMaxPct,
        limits.MarginCashMaxPct,
        limits.PhysicalConcentrationMaxPct,
        limits.FinancialConcentrationMaxPct,
        limits.LogisticsDeadlineMonths,
        limits.FreightCeilingPct,
        limits.CoveredCallMaxPct,
        limits.Contingency1MonthPct,
        limits.Contingency6MonthsPct,
        limits.Contingency12MonthsPct,
        limits.Contingency24MonthsPct,
        limits.Contingency36MonthsPct,
        limits.BuybackTriggerPct,
        limits.BuybackDeadlineBusinessDays,
        limits.StressSigmas,
        limits.StressDays,
        limits.PricingHotPercentile,
        limits.PricingColdPercentile,
        limits.MixShiftMaxPp,
        limits.ConfirmationDeadlineBusinessDays,
        limits.RegistrationDeadlineDays,
        limits.DeviationReportHours);

    public static PolicyLimits ToDomain(this PolicyLimitsDto limits) => PolicyLimits.Create(new PolicyLimitsValues(
        limits.HedgeHorizonYears,
        limits.AbsoluteCeilingPct,
        limits.FxFixedMinPct,
        limits.FxFixedMaxPct,
        limits.FxUnfixedMaxPct,
        limits.MarginCashMaxPct,
        limits.PhysicalConcentrationMaxPct,
        limits.FinancialConcentrationMaxPct,
        limits.LogisticsDeadlineMonths,
        limits.FreightCeilingPct,
        limits.CoveredCallMaxPct,
        limits.Contingency1MonthPct,
        limits.Contingency6MonthsPct,
        limits.Contingency12MonthsPct,
        limits.Contingency24MonthsPct,
        limits.Contingency36MonthsPct,
        limits.BuybackTriggerPct,
        limits.BuybackDeadlineBusinessDays,
        limits.StressSigmas,
        limits.StressDays,
        limits.PricingHotPercentile,
        limits.PricingColdPercentile,
        limits.MixShiftMaxPp,
        limits.ConfirmationDeadlineBusinessDays,
        limits.RegistrationDeadlineDays,
        limits.DeviationReportHours));
}

