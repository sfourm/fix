namespace Fix.Application.Organizations.Dtos;

public sealed record FinancialsDto(
    decimal? Cash,
    decimal? CreditLines,
    decimal? MonthlyFixedCost,
    decimal? NetDebt,
    decimal? Ebitda,
    decimal? UsdDebt,
    DateOnly? ReferenceDate,
    decimal? Leverage);
