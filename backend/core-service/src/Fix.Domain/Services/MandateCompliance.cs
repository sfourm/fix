using System.Globalization;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Domain.Services;

/// <summary>
/// Enquadramento de um mandato na política-mãe (dentro/FORA). Lê a política vigente e o orçamento da companhia:
/// vigência (§15), horizonte de hedge (§7.1), piso econômico e gatilho de preço (§9) e janela dentro da vigência.
/// </summary>
public static class MandateCompliance
{
    private const string PoundPriceUnit = "c/lb";

    public static Compliance Evaluate(Policy policy, Budget budget, MandateType type, MandateTerms terms, DateOnly today)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");

        if (!policy.IsInForceOn(today))
        {
            return Compliance.Outside($"política {policy.Code} {policy.Version} não está vigente — mandato exige aprovação de exceção");
        }

        if (terms.Tenor is not null)
        {
            var months = terms.Tenor.MonthsFrom(today);
            var horizon = policy.Limits.HedgeHorizonYears * 12;
            if (months > horizon)
            {
                return Compliance.Outside($"tela {terms.Tenor} a {months} meses — além do horizonte de {policy.Limits.HedgeHorizonYears} anos (§7.1)");
            }
        }

        if (terms.WindowEnd is not null && policy.Validity.EndsOn is not null && terms.WindowEnd > policy.Validity.EndsOn)
        {
            return Compliance.Outside($"janela termina após a vigência da política ({policy.Validity.EndsOn:dd/MM/yyyy})");
        }

        var isPoundPrice = string.Equals(terms.Price.Unit, PoundPriceUnit, StringComparison.OrdinalIgnoreCase);
        if (type == MandateType.Pricing && !terms.Price.AtMarket && isPoundPrice && terms.Price.Min is { } min)
        {
            if (budget.EconomicFloor is { } floor && min < floor)
            {
                return Compliance.Outside(
                    $"preço mínimo {min.ToString("N2", culture)} c/lb abaixo do piso econômico ({floor.ToString("N2", culture)} c/lb) — exige Comitê (§9)");
            }

            if (budget.CashCost is { } cashCost && min < cashCost)
            {
                return Compliance.Outside(
                    $"preço mínimo {min.ToString("N2", culture)} c/lb abaixo do gatilho (custo caixa {cashCost.ToString("N2", culture)} c/lb)");
            }
        }

        return Compliance.Within(
            $"dentro da política {policy.Code} {policy.Version} (horizonte {policy.Limits.HedgeHorizonYears} anos" +
            (budget.CashCost is { } gatilho ? $" · gatilho {gatilho.ToString("N2", culture)} c/lb" : string.Empty) + ")");
    }
}

