using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Dtos;

public sealed record PolicySummaryDto(
    Guid Id,
    string Code,
    string Title,
    string Version,
    PolicyStatus Status,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    int AxesCount);

