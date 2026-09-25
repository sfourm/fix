using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>Commodity operada pela companhia, com capacidade anual e referência de preço.</summary>
public sealed class OrganizationCommodity : Entity
{
    private OrganizationCommodity()
    {
    }

    internal OrganizationCommodity(Guid organizationId, Commodity commodity)
    {
        OrganizationId = organizationId;
        Commodity = commodity;
    }

    public Guid OrganizationId { get; private set; }

    public Commodity Commodity { get; private set; }

    public decimal Capacity { get; private set; }

    public MeasurementUnit Unit { get; private set; }

    /// <summary>Bolsa / referência de preço (ex.: NY11 · ICE).</summary>
    public string? PriceReference { get; private set; }

    /// <summary>Moeda de referência (ISO 4217, ex.: USD, BRL).</summary>
    public string Currency { get; private set; } = null!;

    /// <summary>A companhia vende esta commodity (senão é insumo/compra).</summary>
    public bool Sells { get; private set; }

    internal void Update(decimal capacity, MeasurementUnit unit, string? priceReference, string currency, bool sells)
    {
        if (capacity < 0)
        {
            throw new DomainException("A capacidade não pode ser negativa.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new DomainException("A moeda deve ser um código ISO de 3 letras (ex.: USD).");
        }

        Capacity = capacity;
        Unit = unit;
        PriceReference = DomainGuard.OptionalText(priceReference, 100, "referência de preço");
        Currency = currency.Trim().ToUpperInvariant();
        Sells = sells;
    }
}

