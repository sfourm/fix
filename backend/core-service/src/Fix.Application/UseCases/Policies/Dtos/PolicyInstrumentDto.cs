using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Dtos;

public sealed record PolicyInstrumentDto(Guid Id, string Name, InstrumentPermission Permission, string? Condition);

