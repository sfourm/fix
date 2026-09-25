using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Dtos;

/// <summary>Política-mãe completa (parâmetros, eixos, bandas, instrumentos e histórico de versões).</summary>
public sealed record PolicyDto(
    Guid Id,
    string Code,
    string Title,
    string Version,
    string? Description,
    PolicyStatus Status,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? ApprovalRecord,
    DateOnly? ApprovedOn,
    PolicyLimitsDto Limits,
    IReadOnlyList<PolicyAxisDto> Axes,
    IReadOnlyList<CoverageBandDto> Bands,
    IReadOnlyList<PolicyInstrumentDto> Instruments,
    IReadOnlyList<PolicyVersionDto> Versions);

