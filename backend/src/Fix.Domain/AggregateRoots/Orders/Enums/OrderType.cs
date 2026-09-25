namespace Fix.Domain.AggregateRoots.Orders;

/// <summary>Tipo da boleta de hedge.</summary>
public enum OrderType
{
    /// <summary>Fixação via futuro em bolsa (ICE/B3/CBOT).</summary>
    Futures = 1,

    /// <summary>Opção vanilla (put/call).</summary>
    Option = 2,

    /// <summary>NDF / termo de moeda.</summary>
    Ndf = 3,
}