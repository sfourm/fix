using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Application.Orders.Commands;

/// <summary>
/// Termos da boleta informados no contrato (registro e edição). Commodity só vale sem mandato; a justificativa é
/// obrigatória quando há desvio (sem mandato, estouro de saldo, tela diferente, venda descoberta).
/// </summary>
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
    string? Notes,
    Commodity? Commodity = null,
    bool CoveredSale = false,
    string? Justification = null);
