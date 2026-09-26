using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>
/// Parâmetros quantitativos da política-mãe (percentuais em %), lidos pelo enquadramento. Os padrões são os do modelo
/// FIX2 (onboarding v60 · POLITICA): limites §5–§8, contingência escalonada, recompra, estresse, régua de fixação,
/// virada de mix e controles.
/// </summary>
public sealed class PolicyLimits : ValueObject
{
    private PolicyLimits()
    {
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

    // ---------- Contingência escalonada (reserva da produção projetada que nunca é vendida nem fixada · I-04) ----------

    /// <summary>Contingência para produção a até 1 mês.</summary>
    public decimal Contingency1MonthPct { get; private set; }

    /// <summary>Contingência para produção a até 6 meses.</summary>
    public decimal Contingency6MonthsPct { get; private set; }

    /// <summary>Contingência para produção a até 12 meses.</summary>
    public decimal Contingency12MonthsPct { get; private set; }

    /// <summary>Contingência para produção a até 24 meses.</summary>
    public decimal Contingency24MonthsPct { get; private set; }

    /// <summary>Contingência para produção a até 36 meses (e além).</summary>
    public decimal Contingency36MonthsPct { get; private set; }

    // ---------- Recompra (over-sold após revisão de guidance) ----------

    /// <summary>Vendido acima deste % do novo disponível obriga recompra.</summary>
    public decimal BuybackTriggerPct { get; private set; }

    /// <summary>Prazo da recompra (dias úteis).</summary>
    public int BuybackDeadlineBusinessDays { get; private set; }

    // ---------- Estresse de caixa (margem) ----------

    /// <summary>Choque em desvios-padrão.</summary>
    public decimal StressSigmas { get; private set; }

    /// <summary>Horizonte do choque (dias úteis).</summary>
    public int StressDays { get; private set; }

    // ---------- Régua de fixação (percentil da série FG/A) ----------

    /// <summary>Mercado "quente" (caro) a partir deste percentil.</summary>
    public int PricingHotPercentile { get; private set; }

    /// <summary>Mercado "frio" (barato) até este percentil.</summary>
    public int PricingColdPercentile { get; private set; }

    // ---------- Mix e controles ----------

    /// <summary>Virada de mix acima destes pontos percentuais exige rito.</summary>
    public decimal MixShiftMaxPp { get; private set; }

    /// <summary>Prazo para o confirmation da contraparte (dias úteis).</summary>
    public int ConfirmationDeadlineBusinessDays { get; private set; }

    /// <summary>Prazo para registrar a boleta após a execução (dias; 0 = D+0).</summary>
    public int RegistrationDeadlineDays { get; private set; }

    /// <summary>Prazo para reportar desvios (horas).</summary>
    public int DeviationReportHours { get; private set; }

    /// <summary>Valores de referência do modelo de política do FIX (onboarding FIX2 v60).</summary>
    public static PolicyLimits Default() => Create(new PolicyLimitsValues(
        HedgeHorizonYears: 2,
        AbsoluteCeilingPct: 120,
        FxFixedMinPct: 80,
        FxFixedMaxPct: 110,
        FxUnfixedMaxPct: 30,
        MarginCashMaxPct: 10,
        PhysicalConcentrationMaxPct: 50,
        FinancialConcentrationMaxPct: 20,
        LogisticsDeadlineMonths: 6,
        FreightCeilingPct: 105,
        CoveredCallMaxPct: 15,
        Contingency1MonthPct: 2,
        Contingency6MonthsPct: 5,
        Contingency12MonthsPct: 10,
        Contingency24MonthsPct: 20,
        Contingency36MonthsPct: 40,
        BuybackTriggerPct: 110,
        BuybackDeadlineBusinessDays: 10,
        StressSigmas: 3,
        StressDays: 10,
        PricingHotPercentile: 70,
        PricingColdPercentile: 30,
        MixShiftMaxPp: 5,
        ConfirmationDeadlineBusinessDays: 2,
        RegistrationDeadlineDays: 0,
        DeviationReportHours: 24));

    public static PolicyLimits Create(PolicyLimitsValues v)
    {
        if (v.HedgeHorizonYears is < 1 or > 10)
        {
            throw new DomainException("O horizonte de hedge deve estar entre 1 e 10 anos.");
        }

        if (v.LogisticsDeadlineMonths is < 0 or > 36)
        {
            throw new DomainException("O prazo-limite de logística deve estar entre 0 e 36 meses.");
        }

        Percentage.Ensure(v.AbsoluteCeilingPct, "teto absoluto", 300);
        Percentage.Ensure(v.FxFixedMinPct, "câmbio mínimo", 300);
        Percentage.Ensure(v.FxFixedMaxPct, "câmbio máximo", 300);
        Percentage.EnsureRange(v.FxFixedMinPct, v.FxFixedMaxPct, "banda de câmbio");
        Percentage.Ensure(v.FxUnfixedMaxPct, "câmbio sobre não fixada");
        Percentage.Ensure(v.MarginCashMaxPct, "margem sobre o caixa");
        Percentage.Ensure(v.PhysicalConcentrationMaxPct, "concentração física");
        Percentage.Ensure(v.FinancialConcentrationMaxPct, "concentração financeira");
        Percentage.Ensure(v.FreightCeilingPct, "teto de frete", 300);
        Percentage.Ensure(v.CoveredCallMaxPct, "venda coberta");

        // A reserva cresce com o horizonte: mais longe, mais incerta a produção.
        decimal[] contingency = [v.Contingency1MonthPct, v.Contingency6MonthsPct, v.Contingency12MonthsPct, v.Contingency24MonthsPct, v.Contingency36MonthsPct];
        foreach (var pct in contingency)
        {
            Percentage.Ensure(pct, "contingência");
        }

        for (var i = 1; i < contingency.Length; i++)
        {
            if (contingency[i] < contingency[i - 1])
            {
                throw new DomainException("A contingência não pode diminuir com o prazo (1 ≤ 6 ≤ 12 ≤ 24 ≤ 36 meses).");
            }
        }

        Percentage.Ensure(v.BuybackTriggerPct, "gatilho de recompra", 300);
        EnsureRange(v.BuybackDeadlineBusinessDays, 1, 60, "prazo de recompra (dias úteis)");
        if (v.StressSigmas is <= 0 or > 10)
        {
            throw new DomainException("O estresse deve estar entre 0 e 10 desvios-padrão.");
        }

        EnsureRange(v.StressDays, 1, 60, "horizonte do estresse (dias)");
        EnsureRange(v.PricingColdPercentile, 0, 100, "percentil frio");
        EnsureRange(v.PricingHotPercentile, 0, 100, "percentil quente");
        if (v.PricingColdPercentile >= v.PricingHotPercentile)
        {
            throw new DomainException("O percentil frio deve ser menor que o quente.");
        }

        Percentage.Ensure(v.MixShiftMaxPp, "virada de mix");
        EnsureRange(v.ConfirmationDeadlineBusinessDays, 0, 30, "prazo da confirmação (dias úteis)");
        EnsureRange(v.RegistrationDeadlineDays, 0, 30, "prazo de registro (dias)");
        EnsureRange(v.DeviationReportHours, 1, 720, "prazo de reporte de desvio (horas)");

        return new PolicyLimits
        {
            HedgeHorizonYears = v.HedgeHorizonYears,
            AbsoluteCeilingPct = v.AbsoluteCeilingPct,
            FxFixedMinPct = v.FxFixedMinPct,
            FxFixedMaxPct = v.FxFixedMaxPct,
            FxUnfixedMaxPct = v.FxUnfixedMaxPct,
            MarginCashMaxPct = v.MarginCashMaxPct,
            PhysicalConcentrationMaxPct = v.PhysicalConcentrationMaxPct,
            FinancialConcentrationMaxPct = v.FinancialConcentrationMaxPct,
            LogisticsDeadlineMonths = v.LogisticsDeadlineMonths,
            FreightCeilingPct = v.FreightCeilingPct,
            CoveredCallMaxPct = v.CoveredCallMaxPct,
            Contingency1MonthPct = v.Contingency1MonthPct,
            Contingency6MonthsPct = v.Contingency6MonthsPct,
            Contingency12MonthsPct = v.Contingency12MonthsPct,
            Contingency24MonthsPct = v.Contingency24MonthsPct,
            Contingency36MonthsPct = v.Contingency36MonthsPct,
            BuybackTriggerPct = v.BuybackTriggerPct,
            BuybackDeadlineBusinessDays = v.BuybackDeadlineBusinessDays,
            StressSigmas = v.StressSigmas,
            StressDays = v.StressDays,
            PricingHotPercentile = v.PricingHotPercentile,
            PricingColdPercentile = v.PricingColdPercentile,
            MixShiftMaxPp = v.MixShiftMaxPp,
            ConfirmationDeadlineBusinessDays = v.ConfirmationDeadlineBusinessDays,
            RegistrationDeadlineDays = v.RegistrationDeadlineDays,
            DeviationReportHours = v.DeviationReportHours,
        };
    }

    public PolicyLimitsValues ToValues() => new(
        HedgeHorizonYears,
        AbsoluteCeilingPct,
        FxFixedMinPct,
        FxFixedMaxPct,
        FxUnfixedMaxPct,
        MarginCashMaxPct,
        PhysicalConcentrationMaxPct,
        FinancialConcentrationMaxPct,
        LogisticsDeadlineMonths,
        FreightCeilingPct,
        CoveredCallMaxPct,
        Contingency1MonthPct,
        Contingency6MonthsPct,
        Contingency12MonthsPct,
        Contingency24MonthsPct,
        Contingency36MonthsPct,
        BuybackTriggerPct,
        BuybackDeadlineBusinessDays,
        StressSigmas,
        StressDays,
        PricingHotPercentile,
        PricingColdPercentile,
        MixShiftMaxPp,
        ConfirmationDeadlineBusinessDays,
        RegistrationDeadlineDays,
        DeviationReportHours);

    private static void EnsureRange(int value, int min, int max, string label)
    {
        if (value < min || value > max)
        {
            throw new DomainException($"O {label} deve estar entre {min} e {max}.");
        }
    }

    // O record já compara por valor todos os parâmetros.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ToValues();
    }
}

/// <summary>Valores dos parâmetros da política (entrada de <see cref="PolicyLimits.Create"/>).</summary>
public sealed record PolicyLimitsValues(
    int HedgeHorizonYears,
    decimal AbsoluteCeilingPct,
    decimal FxFixedMinPct,
    decimal FxFixedMaxPct,
    decimal FxUnfixedMaxPct,
    decimal MarginCashMaxPct,
    decimal PhysicalConcentrationMaxPct,
    decimal FinancialConcentrationMaxPct,
    int LogisticsDeadlineMonths,
    decimal FreightCeilingPct,
    decimal CoveredCallMaxPct,
    decimal Contingency1MonthPct,
    decimal Contingency6MonthsPct,
    decimal Contingency12MonthsPct,
    decimal Contingency24MonthsPct,
    decimal Contingency36MonthsPct,
    decimal BuybackTriggerPct,
    int BuybackDeadlineBusinessDays,
    decimal StressSigmas,
    int StressDays,
    int PricingHotPercentile,
    int PricingColdPercentile,
    decimal MixShiftMaxPp,
    int ConfirmationDeadlineBusinessDays,
    int RegistrationDeadlineDays,
    int DeviationReportHours);
