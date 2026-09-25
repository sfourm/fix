namespace Fix.Application.Policies.Dtos;

/// <summary>Parâmetros quantitativos da política (percentuais em %).</summary>
public sealed record PolicyLimitsDto(
    int HedgeHorizonYears,
    decimal AbsoluteCeilingPct,
    decimal FxFixedMinPct,
    decimal FxFixedMaxPct,
    decimal FxUnfixedMaxPct,
    decimal MarginCashMaxPct,
    decimal PhysicalConcentrationMaxPct,
    decimal FinancialConcentrationMaxPct,
    int LogisticsDeadlineMonths,
    decimal FreightCeilingPct,
    decimal CoveredCallMaxPct);
