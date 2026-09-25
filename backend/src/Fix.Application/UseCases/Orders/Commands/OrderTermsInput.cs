using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Application.Orders.Commands;

/// <summary>Termos da boleta informados no contrato (registro e edição).</summary>
public sealed record OrderTermsInput(
    OrderType Type,
    TradeDirection Direction,
    string Tenor,
    decimal? Lots,
    decimal? NotionalUsd,
    decimal Price,
    string? PriceUnit,
    OptionKind? OptionKind,
    decimal? Premium,
    DateOnly TradeDate,
    string? Notes);

