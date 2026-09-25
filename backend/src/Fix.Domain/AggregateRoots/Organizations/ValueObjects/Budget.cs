using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>
/// Orçamento e gatilhos de preço (em ¢/lb-equivalente): o custo caixa é o gatilho de fixação
/// e o piso econômico é o limite abaixo do qual a fixação exige deliberação do Comitê.
/// </summary>
public sealed class Budget : ValueObject
{
    public const string PriceUnit = "c/lb";

    private Budget()
    {
    }

    private Budget(decimal? cashCost, decimal? economicFloor, decimal? equivalentPrice, decimal? targetMarginPct)
    {
        CashCost = cashCost;
        EconomicFloor = economicFloor;
        EquivalentPrice = equivalentPrice;
        TargetMarginPct = targetMarginPct;
    }

    /// <summary>Custo operacional caixa · gatilho de fixação.</summary>
    public decimal? CashCost { get; private set; }

    /// <summary>Piso econômico · abaixo dele a fixação é FORA da política.</summary>
    public decimal? EconomicFloor { get; private set; }

    /// <summary>Preço equivalente orçado.</summary>
    public decimal? EquivalentPrice { get; private set; }

    public decimal? TargetMarginPct { get; private set; }

    public static Budget Empty() => new(null, null, null, null);

    public static Budget Create(decimal? cashCost, decimal? economicFloor, decimal? equivalentPrice, decimal? targetMarginPct)
    {
        if (cashCost < 0 || economicFloor < 0 || equivalentPrice < 0)
        {
            throw new DomainException("Os preços do orçamento não podem ser negativos.");
        }

        return new Budget(cashCost, economicFloor, equivalentPrice, targetMarginPct);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CashCost;
        yield return EconomicFloor;
        yield return EquivalentPrice;
        yield return TargetMarginPct;
    }
}

