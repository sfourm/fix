using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

public sealed record OrganizationCreatedDomainEvent(Guid OrganizationId, Guid FounderUserId) : IDomainEvent;

