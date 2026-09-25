using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>
/// Fotografia financeira (R$ e US$). Não aparece como tela própria no FIX, mas alimenta o enquadramento:
/// teto de margem sobre o caixa, estresse de liquidez, alavancagem e dívida em US$ como hedge natural.
/// </summary>
public sealed class Financials : ValueObject
{
    private Financials()
    {
    }

    private Financials(
        decimal? cash,
        decimal? creditLines,
        decimal? monthlyFixedCost,
        decimal? netDebt,
        decimal? ebitda,
        decimal? usdDebt,
        DateOnly? referenceDate)
    {
        Cash = cash;
        CreditLines = creditLines;
        MonthlyFixedCost = monthlyFixedCost;
        NetDebt = netDebt;
        Ebitda = ebitda;
        UsdDebt = usdDebt;
        ReferenceDate = referenceDate;
    }

    public decimal? Cash { get; private set; }

    public decimal? CreditLines { get; private set; }

    public decimal? MonthlyFixedCost { get; private set; }

    public decimal? NetDebt { get; private set; }

    public decimal? Ebitda { get; private set; }

    public decimal? UsdDebt { get; private set; }

    public DateOnly? ReferenceDate { get; private set; }

    /// <summary>Dívida líquida / EBITDA.</summary>
    public decimal? Leverage => NetDebt is not null && Ebitda is > 0 ? NetDebt / Ebitda : null;

    public static Financials Empty() => new(null, null, null, null, null, null, null);

    public static Financials Create(
        decimal? cash,
        decimal? creditLines,
        decimal? monthlyFixedCost,
        decimal? netDebt,
        decimal? ebitda,
        decimal? usdDebt,
        DateOnly? referenceDate)
    {
        if (cash < 0 || creditLines < 0 || monthlyFixedCost < 0 || usdDebt < 0)
        {
            throw new DomainException("Caixa, linhas, custo fixo e dívida em US$ não podem ser negativos.");
        }

        return new Financials(cash, creditLines, monthlyFixedCost, netDebt, ebitda, usdDebt, referenceDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Cash;
        yield return CreditLines;
        yield return MonthlyFixedCost;
        yield return NetDebt;
        yield return Ebitda;
        yield return UsdDebt;
        yield return ReferenceDate;
    }
}

