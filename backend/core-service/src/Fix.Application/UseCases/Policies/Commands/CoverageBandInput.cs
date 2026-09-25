namespace Fix.Application.Policies.Commands;

/// <summary>Dados de uma banda de cobertura (inclusão/edição).</summary>
public sealed record CoverageBandInput(string Horizon, string Crop, decimal MinPct, decimal MaxPct, string? Note);
