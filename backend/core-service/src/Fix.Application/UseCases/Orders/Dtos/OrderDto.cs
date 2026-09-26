using Fix.Application.Mandates.Dtos;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Application.Orders.Dtos;

/// <summary>Boleta de hedge, com enquadramento e carimbos de desvio (sem mandato, estouro, a posteriori).</summary>
public sealed record OrderDto(
    Guid Id,
    string Code,
    Guid? MandateId,
    string? MandateCode,
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
    bool CoveredSale,
    DateOnly TradeDate,
    string? Notes,
    ComplianceDto Compliance,
    bool LinkedAfterExecution,
    bool ExceedsMandate,
    string? DeviationNote,
    ApprovalStatus Approval,
    Guid RequestedBy,
    Guid? DecidedBy,
    DateTimeOffset? DecidedAt,
    string? DecisionNote,
    ConfirmationStatus Confirmation,
    DateOnly? ConfirmedOn,
    string? ConfirmationNote,
    Guid? ConfirmationBy,
    bool ConfirmationOverdue);
