using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Commands;

/// <summary>Dados de um eixo da política (inclusão/edição).</summary>
public sealed record PolicyAxisInput(
    string Code,
    string Title,
    RiskFactor Factor,
    string? Statement,
    string? LimitDescription,
    string? Approver,
    IReadOnlyList<string> Restrictions);

