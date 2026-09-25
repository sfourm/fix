namespace Fix.Application.Organizations.Dtos;

/// <summary>Orçamento e gatilhos em ¢/lb-equivalente.</summary>
public sealed record BudgetDto(
    decimal? CashCost,
    decimal? EconomicFloor,
    decimal? EquivalentPrice,
    decimal? TargetMarginPct);
