using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;

namespace Fix.Application.Mandates.Dtos;

public sealed record MandateDto(
    Guid Id,
    Guid PolicyId,
    string PolicyCode,
    string PolicyVersion,
    Guid AxisId,
    string AxisCode,
    string AxisTitle,
    MandateType Type,
    string Title,
    string? Criteria,
    Commodity? Commodity,
    string? Tenor,
    decimal? Quantity,
    MeasurementUnit? QuantityUnit,
    // Consumido por boletas aprovadas (lotes ou US$).
    decimal Consumed,
    // Saldo disponível; nulo quando o mandato não tem teto de volume.
    decimal? Balance,
    PriceCriteriaDto Price,
    DateOnly? WindowStart,
    DateOnly? WindowEnd,
    ComplianceDto Compliance,
    MandateStatus Status,
    Guid IssuedBy,
    Guid? DecidedBy,
    DateTimeOffset? DecidedAt,
    string? DecisionNote);

