using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Abstractions.Timeline;

namespace Fix.Application.Timeline.Queries;

/// <summary>
/// Histórico de alterações da organização. Filtra opcionalmente por tipo (ex.: "Policy") e id da entidade.
/// </summary>
[RequireMembership]
public sealed record GetTimelineQuery(Guid UserId, Guid OrganizationId, string? EntityType, Guid? EntityId, int Limit)
    : IQuery<IReadOnlyList<TimelineEntryDto>>, IOrganizationRequest;
