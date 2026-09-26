using Fix.Application.Abstractions.Timeline;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Timeline.Queries;

namespace Fix.Application.Timeline.Services;

internal sealed class TimelineService(ITimelineReader timelineReader)
    : ITimelineService
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 500;
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 200;
    private static readonly string[] Actions = ["Created", "Updated", "Deleted"];

    public async Task<TimelinePage> GetTimelineAsync(GetTimelineQuery query, CancellationToken cancellationToken)
    {
        var paged = query.Page > 0;
        var pageSize = paged
            ? (query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize))
            : (query.Limit <= 0 ? DefaultLimit : Math.Min(query.Limit, MaxLimit));
        var page = paged ? query.Page : 1;
        var action = Actions.FirstOrDefault(a => string.Equals(a, query.Action?.Trim(), StringComparison.OrdinalIgnoreCase));

        var filter = new TimelineFilter(
            EntityType: Blank(query.EntityType),
            EntityId: query.EntityId,
            Action: action,
            AuthorId: query.AuthorId,
            From: query.From,
            To: query.To,
            Search: Blank(query.Search),
            Skip: (page - 1) * pageSize,
            Take: pageSize,
            CountTotal: paged);

        var (entries, total) = await timelineReader.ListAsync(query.OrganizationId, filter, cancellationToken);
        return new TimelinePage(entries, total, page, pageSize);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
