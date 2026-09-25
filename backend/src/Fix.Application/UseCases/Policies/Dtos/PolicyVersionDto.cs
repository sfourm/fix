using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Policies.Dtos;

public sealed record PolicyVersionDto(string Version, PolicyStatus Status, DateOnly Date, string? Note);

