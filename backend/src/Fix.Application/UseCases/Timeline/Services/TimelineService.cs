using Fix.Application.Abstractions.Timeline;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Timeline.Queries;

namespace Fix.Application.Timeline.Services;

internal sealed class TimelineService(ITimelineReader timelineReader)
    : ITimelineService
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 500;

    public Task<IReadOnlyList<TimelineEntryDto>> GetTimelineAsync(GetTimelineQuery query, CancellationToken cancellationToken)
    {
        var limit = query.Limit <= 0 ? DefaultLimit : Math.Min(query.Limit, MaxLimit);
        var entityType = string.IsNullOrWhiteSpace(query.EntityType) ? null : query.EntityType.Trim();

        return timelineReader.ListAsync(query.OrganizationId, entityType, query.EntityId, limit, cancellationToken);
    }
}
