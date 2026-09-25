using Fix.Domain.Common;

namespace Fix.Application.Mandates.Commands;

/// <summary>Termos do mandato informados no contrato (emissão, edição e prévia de enquadramento).</summary>
public sealed record MandateTermsInput(
    string Title,
    string? Criteria,
    Commodity? Commodity,
    string? Tenor,
    decimal? Quantity,
    MeasurementUnit? QuantityUnit,
    bool AtMarket,
    decimal? PriceTarget,
    decimal? PriceMin,
    decimal? PriceMax,
    string? PriceUnit,
    DateOnly? WindowStart,
    DateOnly? WindowEnd);
