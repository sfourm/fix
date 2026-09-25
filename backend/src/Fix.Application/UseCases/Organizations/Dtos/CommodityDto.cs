using Fix.Domain.Common;

namespace Fix.Application.Organizations.Dtos;

public sealed record CommodityDto(
    Guid Id,
    Commodity Commodity,
    decimal Capacity,
    MeasurementUnit Unit,
    string? PriceReference,
    string Currency,
    bool Sells);
