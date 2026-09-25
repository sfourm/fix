namespace Fix.Domain.Common;

public enum MeasurementUnit
{
    /// <summary>Lotes de bolsa (NY11: 112.000 lb ≈ 50,8 t).</summary>
    Lots = 1,
    Tonnes = 2,

    /// <summary>Sacas de 60 kg.</summary>
    Bags = 3,
    CubicMeters = 4,
    Pounds = 5,

    /// <summary>Valor nocional em dólares (mandatos e boletas de moeda).</summary>
    Usd = 6,
}
