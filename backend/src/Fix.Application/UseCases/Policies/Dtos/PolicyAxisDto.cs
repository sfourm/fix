using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Dtos;

public sealed record PolicyAxisDto(
    Guid Id,
    string Code,
    string Title,
    RiskFactor Factor,
    string? Statement,
    string? LimitDescription,
    string? Approver,
    IReadOnlyList<string> Restrictions);

