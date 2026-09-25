using Fix.Application.Abstractions.Messaging;
using Fix.Application.Abstractions.Timeline;
using Fix.Application.Timeline.Queries;

namespace Fix.Application.Timeline.Services;

internal sealed class TimelineService(ITimelineReader timelineReader)
    : IQueryHandler<GetTimelineQuery, IReadOnlyList<TimelineEntryDto>>
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 500;

    public Task<IReadOnlyList<TimelineEntryDto>> HandleAsync(GetTimelineQuery query, CancellationToken cancellationToken)
    {
        var limit = query.Limit <= 0 ? DefaultLimit : Math.Min(query.Limit, MaxLimit);
        var entityType = string.IsNullOrWhiteSpace(query.EntityType) ? null : query.EntityType.Trim();

        return timelineReader.ListAsync(query.OrganizationId, entityType, query.EntityId, limit, cancellationToken);
    }
}
