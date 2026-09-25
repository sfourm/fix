namespace Fix.Application.Mandates.Dtos;

public sealed record PriceCriteriaDto(bool AtMarket, decimal? Target, decimal? Min, decimal? Max, string? Unit);
