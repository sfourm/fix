using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Application.Orders.Dtos;

/// <summary>Boleta de hedge.</summary>
public sealed record OrderDto(
    Guid Id,
    Guid MandateId,
    string MandateTitle,
    OrderType Type,
    TradeDirection Direction,
    Guid CounterpartyId,
    string CounterpartyName,
    Commodity? Commodity,
    string Tenor,
    decimal? Lots,
    decimal? NotionalUsd,
    decimal Price,
    string PriceUnit,
    OptionKind? OptionKind,
    decimal? Premium,
    DateOnly TradeDate,
    string? Notes,
    ApprovalStatus Approval,
    Guid RequestedBy,
    Guid? DecidedBy,
    DateTimeOffset? DecidedAt,
    string? DecisionNote,
    ConfirmationStatus Confirmation,
    DateOnly? ConfirmedOn,
    string? ConfirmationNote,
    bool ConfirmationOverdue);

