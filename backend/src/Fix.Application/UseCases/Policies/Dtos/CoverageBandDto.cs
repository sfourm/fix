namespace Fix.Application.Policies.Dtos;

public sealed record CoverageBandDto(Guid Id, string Horizon, string Crop, decimal MinPct, decimal MaxPct, string? Note);
