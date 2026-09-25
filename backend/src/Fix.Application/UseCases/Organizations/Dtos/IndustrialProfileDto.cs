namespace Fix.Application.Organizations.Dtos;

public sealed record IndustrialProfileDto(
    decimal? MillingCapacity,
    decimal? MixMinPct,
    decimal? MixMaxPct,
    decimal? MixGuidancePct);
