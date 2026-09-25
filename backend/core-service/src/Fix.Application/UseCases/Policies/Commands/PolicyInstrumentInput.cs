using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Commands;

/// <summary>Dados de um instrumento da política (inclusão/edição).</summary>
public sealed record PolicyInstrumentInput(string Name, InstrumentPermission Permission, string? Condition);

