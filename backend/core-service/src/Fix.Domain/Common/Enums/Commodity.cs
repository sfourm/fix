namespace Fix.Domain.Common;

/// <summary>Commodities negociadas (cada uma define bolsa/referência de preço).</summary>
public enum Commodity
{
    /// <summary>Açúcar VHP · NY11 (ICE).</summary>
    RawSugar = 1,

    /// <summary>Açúcar branco · Londres nº5 (ICE).</summary>
    WhiteSugar = 2,

    /// <summary>Etanol hidratado · B3/ESALQ.</summary>
    HydratedEthanol = 3,

    /// <summary>Etanol anidro · B3/ESALQ.</summary>
    AnhydrousEthanol = 4,

    /// <summary>Milho · CBOT.</summary>
    Corn = 5,

    /// <summary>Soja · CBOT.</summary>
    Soybean = 6,
}
