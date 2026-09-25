namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>Fator de risco coberto por um eixo da política.</summary>
public enum RiskFactor
{
    /// <summary>Físico (venda do produto, programação de entregas).</summary>
    Physical = 1,

    /// <summary>Preço da commodity (fixação).</summary>
    Price = 2,

    /// <summary>Moeda (proteção cambial da receita).</summary>
    Currency = 3,

    /// <summary>Frete e logística.</summary>
    Freight = 4,
}

