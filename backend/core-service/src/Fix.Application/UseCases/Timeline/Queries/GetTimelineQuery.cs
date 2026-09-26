using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Abstractions.Timeline;

namespace Fix.Application.Timeline.Queries;

/// <summary>
/// Auditoria da organização. Filtros opcionais: tipo e id da entidade, ação, autor, período (de inclusive, até
/// exclusive) e texto nos valores alterados. Com <paramref name="Page"/> &gt; 0 vem paginada (com o total);
/// sem página, traz os <paramref name="Limit"/> mais recentes.
/// </summary>
[RequireMembership]
public sealed record GetTimelineQuery(
    Guid UserId,
    Guid OrganizationId,
    string? EntityType,
    Guid? EntityId,
    int Limit,
    string? Action = null,
    Guid? AuthorId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Search = null,
    int Page = 0,
    int PageSize = 0)
    : IQuery<TimelinePage>, IOrganizationRequest;
