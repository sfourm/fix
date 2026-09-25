using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>Parâmetros quantitativos da política-mãe (percentuais em %).</summary>
public sealed class PolicyLimits : ValueObject
{
    private PolicyLimits()
    {
    }

    private PolicyLimits(
        int hedgeHorizonYears,
        decimal absoluteCeilingPct,
        decimal fxFixedMinPct,
        decimal fxFixedMaxPct,
        decimal fxUnfixedMaxPct,
        decimal marginCashMaxPct,
        decimal physicalConcentrationMaxPct,
        decimal financialConcentrationMaxPct,
        int logisticsDeadlineMonths,
        decimal freightCeilingPct,
        decimal coveredCallMaxPct)
    {
        HedgeHorizonYears = hedgeHorizonYears;
        AbsoluteCeilingPct = absoluteCeilingPct;
        FxFixedMinPct = fxFixedMinPct;
        FxFixedMaxPct = fxFixedMaxPct;
        FxUnfixedMaxPct = fxUnfixedMaxPct;
        MarginCashMaxPct = marginCashMaxPct;
        PhysicalConcentrationMaxPct = physicalConcentrationMaxPct;
        FinancialConcentrationMaxPct = financialConcentrationMaxPct;
        LogisticsDeadlineMonths = logisticsDeadlineMonths;
        FreightCeilingPct = freightCeilingPct;
        CoveredCallMaxPct = coveredCallMaxPct;
    }

    /// <summary>Horizonte máximo de hedge (anos) · §7.1.</summary>
    public int HedgeHorizonYears { get; private set; }

    /// <summary>Teto absoluto de cobertura sobre o disponível · §6.3.</summary>
    public decimal AbsoluteCeilingPct { get; private set; }

    /// <summary>Banda mínima de NDF sobre a receita fixada · §6.2.</summary>
    public decimal FxFixedMinPct { get; private set; }

    /// <summary>Banda máxima de NDF sobre a receita fixada · §6.2.</summary>
    public decimal FxFixedMaxPct { get; private set; }

    /// <summary>Teto de proteção antecipada sobre a receita não fixada · §6.2.</summary>
    public decimal FxUnfixedMaxPct { get; private set; }

    /// <summary>Margem em corretoras sobre o caixa · §6.4.</summary>
    public decimal MarginCashMaxPct { get; private set; }

    /// <summary>Concentração máxima por contraparte no físico · §6.5.</summary>
    public decimal PhysicalConcentrationMaxPct { get; private set; }

    /// <summary>Concentração máxima por contraparte no financeiro OTC · §6.5.</summary>
    public decimal FinancialConcentrationMaxPct { get; private set; }

    /// <summary>Prazo-limite para contratar o frete das entregas (meses) · §7.2.</summary>
    public int LogisticsDeadlineMonths { get; private set; }

    /// <summary>Tarifa máxima de frete sobre a referência de mercado · §5.4.</summary>
    public decimal FreightCeilingPct { get; private set; }

    /// <summary>Teto de venda coberta de opções sobre o disponível · §8.</summary>
    public decimal CoveredCallMaxPct { get; private set; }

    /// <summary>Valores de referência do modelo de política do FIX.</summary>
    public static PolicyLimits Default() => new(2, 120, 80, 110, 30, 10, 50, 20, 6, 105, 15);

    public static PolicyLimits Create(
        int hedgeHorizonYears,
        decimal absoluteCeilingPct,
        decimal fxFixedMinPct,
        decimal fxFixedMaxPct,
        decimal fxUnfixedMaxPct,
        decimal marginCashMaxPct,
        decimal physicalConcentrationMaxPct,
        decimal financialConcentrationMaxPct,
        int logisticsDeadlineMonths,
        decimal freightCeilingPct,
        decimal coveredCallMaxPct)
    {
        if (hedgeHorizonYears is < 1 or > 10)
        {
            throw new DomainException("O horizonte de hedge deve estar entre 1 e 10 anos.");
        }

        if (logisticsDeadlineMonths is < 0 or > 36)
        {
            throw new DomainException("O prazo-limite de logística deve estar entre 0 e 36 meses.");
        }

        Percentage.Ensure(absoluteCeilingPct, "teto absoluto", 300);
        Percentage.Ensure(fxFixedMinPct, "câmbio mínimo", 300);
        Percentage.Ensure(fxFixedMaxPct, "câmbio máximo", 300);
        Percentage.EnsureRange(fxFixedMinPct, fxFixedMaxPct, "banda de câmbio");
        Percentage.Ensure(fxUnfixedMaxPct, "câmbio sobre não fixada");
        Percentage.Ensure(marginCashMaxPct, "margem sobre o caixa");
        Percentage.Ensure(physicalConcentrationMaxPct, "concentração física");
        Percentage.Ensure(financialConcentrationMaxPct, "concentração financeira");
        Percentage.Ensure(freightCeilingPct, "teto de frete", 300);
        Percentage.Ensure(coveredCallMaxPct, "venda coberta");

        return new PolicyLimits(
            hedgeHorizonYears,
            absoluteCeilingPct,
            fxFixedMinPct,
            fxFixedMaxPct,
            fxUnfixedMaxPct,
            marginCashMaxPct,
            physicalConcentrationMaxPct,
            financialConcentrationMaxPct,
            logisticsDeadlineMonths,
            freightCeilingPct,
            coveredCallMaxPct);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HedgeHorizonYears;
        yield return AbsoluteCeilingPct;
        yield return FxFixedMinPct;
        yield return FxFixedMaxPct;
        yield return FxUnfixedMaxPct;
        yield return MarginCashMaxPct;
        yield return PhysicalConcentrationMaxPct;
        yield return FinancialConcentrationMaxPct;
        yield return LogisticsDeadlineMonths;
        yield return FreightCeilingPct;
        yield return CoveredCallMaxPct;
    }
}

