using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>Capacidade industrial e banda de mix açúcar × etanol.</summary>
public sealed class IndustrialProfile : ValueObject
{
    private IndustrialProfile()
    {
    }

    private IndustrialProfile(decimal? millingCapacity, decimal? mixMinPct, decimal? mixMaxPct, decimal? mixGuidancePct)
    {
        MillingCapacity = millingCapacity;
        MixMinPct = mixMinPct;
        MixMaxPct = mixMaxPct;
        MixGuidancePct = mixGuidancePct;
    }

    /// <summary>Moagem anual em toneladas de cana.</summary>
    public decimal? MillingCapacity { get; private set; }

    public decimal? MixMinPct { get; private set; }

    public decimal? MixMaxPct { get; private set; }

    public decimal? MixGuidancePct { get; private set; }

    public static IndustrialProfile Empty() => new(null, null, null, null);

    public static IndustrialProfile Create(decimal? millingCapacity, decimal? mixMinPct, decimal? mixMaxPct, decimal? mixGuidancePct)
    {
        if (millingCapacity < 0)
        {
            throw new DomainException("A capacidade de moagem não pode ser negativa.");
        }

        Percentage.EnsureOptional(mixMinPct, "mix mínimo");
        Percentage.EnsureOptional(mixMaxPct, "mix máximo");
        Percentage.EnsureOptional(mixGuidancePct, "mix guidance");
        Percentage.EnsureRange(mixMinPct, mixMaxPct, "mix");

        if (mixGuidancePct is not null && ((mixMinPct is not null && mixGuidancePct < mixMinPct) || (mixMaxPct is not null && mixGuidancePct > mixMaxPct)))
        {
            throw new DomainException("O mix guidance deve estar dentro da banda industrial.");
        }

        return new IndustrialProfile(millingCapacity, mixMinPct, mixMaxPct, mixGuidancePct);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MillingCapacity;
        yield return MixMinPct;
        yield return MixMaxPct;
        yield return MixGuidancePct;
    }
}

