using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>
/// Modelo de política do FIX (eixos, instrumentos e bandas de referência) usado para iniciar uma nova política.
/// </summary>
public static class PolicyTemplate
{
    public static void Apply(Policy policy, CropYear? activeCrop)
    {
        policy.AddAxis("POL-CTB", Title.Create("Venda do físico"), RiskFactor.Physical,
            "Vender o físico da safra em contratos com contrapartes aprovadas.", null, "Conselho de Administração",
            ["contrapartes: tradings aprovadas pelo Conselho", "tenor máximo: 3 safras"]);
        policy.AddAxis("POL-PRE", Title.Create("Fixação de preço"), RiskFactor.Price,
            "Fixar o preço dentro das bandas de cobertura e nunca abaixo do piso econômico.", null, "Conselho de Administração",
            ["instrumentos: futuro, opção vanilla, swap BRL", "piso: piso econômico do orçamento (§9)"]);
        policy.AddAxis("POL-FX", Title.Create("Proteção cambial da receita"), RiskFactor.Currency,
            "Proteger a receita exportadora fixada dentro da banda de câmbio.", null, "Conselho de Administração",
            ["instrumentos: NDF e opção de câmbio", "contrapartes: bancos com limite aprovado", "fixing: PTAX"]);
        policy.AddAxis("POL-FRT", Title.Create("Contratação de frete e logística"), RiskFactor.Freight,
            "Contratar frete até o teto sobre a referência de mercado, em cadência compatível com as entregas.", null, "Diretoria Executiva",
            ["transportadoras homologadas"]);
        policy.AddAxis("POL-ENT", Title.Create("Programação de entregas"), RiskFactor.Physical,
            "Cumprir a cadência contratual de entregas; desvios acima de 10% exigem justificativa.", null, "Diretoria Executiva",
            []);

        policy.AddInstrument("Futuros em bolsa (ICE / B3 / CBOT)", InstrumentPermission.Allowed, "dentro das bandas e da janela");
        policy.AddInstrument("NDF / termo de moeda", InstrumentPermission.Allowed, "contraparte homologada · fixing PTAX");
        policy.AddInstrument("Compra de opções vanilla (put/call)", InstrumentPermission.Allowed, "prêmio orçado · delta-equivalente nos limites");
        policy.AddInstrument("Venda coberta de opções", InstrumentPermission.Capped, "até o teto de venda coberta · aprovação do Comitê");
        policy.AddInstrument("Venda descoberta de opções", InstrumentPermission.Forbidden, null);
        policy.AddInstrument("Estruturas exóticas (acumuladores, barreiras, alavancadas)", InstrumentPermission.Forbidden, "só por revisão da Política / Conselho");
        policy.AddInstrument("Swap de commodity em BRL (tela + câmbio)", InstrumentPermission.Allowed, null);
        policy.AddInstrument("Contratos comerciais a preço fixo", InstrumentPermission.Allowed, "contam como cobertura nas bandas");

        if (activeCrop is null)
        {
            return;
        }

        policy.AddBand("Safra corrente", activeCrop, 60, 120, "sobre o disponível (produção − contingência)");
        policy.AddBand("Safra +1", Next(activeCrop, 1), 30, 80, "habilitada após guidance agrícola");
        policy.AddBand("Safra +2", Next(activeCrop, 2), 0, 40, "somente com aprovação do Comitê");
    }

    private static CropYear Next(CropYear crop, int years)
    {
        var start = crop.StartYear + years;
        return CropYear.Create($"{start % 100:00}/{(start + 1) % 100:00}");
    }
}

