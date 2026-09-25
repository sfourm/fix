using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Domain.AggregateRoots.Mandates;

public enum MandateType
{
    /// <summary>Precificação da commodity (fixação) · eixo de preço.</summary>
    Pricing = 1,

    /// <summary>Proteção cambial · eixo de moeda.</summary>
    Currency = 2,

    /// <summary>Operação comercial (venda/compra do físico) · eixo físico.</summary>
    Commercial = 3,

    /// <summary>Operação logística (frete) · eixo de frete.</summary>
    Logistics = 4,
}

public static class MandateTypeExtensions
{
    /// <summary>Fator de risco que o eixo do mandato precisa cobrir.</summary>
    public static RiskFactor Factor(this MandateType type) => type switch
    {
        MandateType.Pricing => RiskFactor.Price,
        MandateType.Currency => RiskFactor.Currency,
        MandateType.Commercial => RiskFactor.Physical,
        _ => RiskFactor.Freight,
    };
}

