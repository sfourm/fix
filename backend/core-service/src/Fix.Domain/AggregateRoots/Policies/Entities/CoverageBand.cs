using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>Banda de cobertura por horizonte de safra (mín–máx % sobre o disponível) · §6.1.</summary>
public sealed class CoverageBand : Entity
{
    private CoverageBand()
    {
    }

    internal CoverageBand(Guid policyId) => PolicyId = policyId;

    public Guid PolicyId { get; private set; }

    /// <summary>Rótulo do horizonte (ex.: Safra corrente, Safra +1).</summary>
    public string Horizon { get; private set; } = null!;

    public CropYear Crop { get; private set; } = null!;

    public decimal MinPct { get; private set; }

    public decimal MaxPct { get; private set; }

    public string? Note { get; private set; }

    internal void Update(string horizon, CropYear crop, decimal minPct, decimal maxPct, string? note)
    {
        Percentage.Ensure(minPct, "cobertura mínima", 300);
        Percentage.Ensure(maxPct, "cobertura máxima", 300);
        Percentage.EnsureRange(minPct, maxPct, "banda de cobertura");

        Horizon = DomainGuard.OptionalText(horizon, 60, "horizonte") ?? throw new DomainException("O horizonte é obrigatório.");
        Crop = crop;
        MinPct = minPct;
        MaxPct = maxPct;
        Note = DomainGuard.OptionalText(note, 300, "observação");
    }
}

