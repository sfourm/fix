using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Mandates;

/// <summary>Critério de preço do mandato: a mercado, ou target com mínimo/máximo, na unidade informada.</summary>
public sealed class PriceCriteria : ValueObject
{
    private PriceCriteria()
    {
    }

    private PriceCriteria(bool atMarket, decimal? target, decimal? min, decimal? max, string? unit)
    {
        AtMarket = atMarket;
        Target = target;
        Min = min;
        Max = max;
        Unit = unit;
    }

    public bool AtMarket { get; private set; }

    public decimal? Target { get; private set; }

    public decimal? Min { get; private set; }

    public decimal? Max { get; private set; }

    /// <summary>Unidade de preço (ex.: c/lb, R$/t, R$/US$).</summary>
    public string? Unit { get; private set; }

    public bool HasAnyLevel => Target is not null || Min is not null || Max is not null;

    public static PriceCriteria None() => new(false, null, null, null, null);

    public static PriceCriteria Create(bool atMarket, decimal? target, decimal? min, decimal? max, string? unit)
    {
        if (atMarket)
        {
            return new PriceCriteria(true, null, null, null, DomainGuard.OptionalText(unit, 20, "unidade de preço"));
        }

        if (target <= 0 || min <= 0 || max <= 0)
        {
            throw new DomainException("Os níveis de preço devem ser positivos.");
        }

        Percentage.EnsureRange(min, max, "critério de preço");
        if (target is not null && ((min is not null && target < min) || (max is not null && target > max)))
        {
            throw new DomainException("O preço target deve estar entre o mínimo e o máximo.");
        }

        return new PriceCriteria(false, target, min, max, DomainGuard.OptionalText(unit, 20, "unidade de preço"));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AtMarket;
        yield return Target;
        yield return Min;
        yield return Max;
        yield return Unit;
    }
}

