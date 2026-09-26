using Fix.Domain.AggregateRoots.Counterparties;

namespace Fix.Application.Counterparties.Dtos;

public sealed record CounterpartyDto(
    Guid Id,
    string Code,
    string Name,
    CounterpartyType Type,
    string? Document,
    string? Address,
    string? Country,
    bool IsHomologated,
    decimal? NotionalLimitUsd,
    decimal? MtmLimitUsd);

