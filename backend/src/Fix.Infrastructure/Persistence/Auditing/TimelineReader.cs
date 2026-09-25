using Fix.Application.Abstractions.Timeline;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Auditing;

internal sealed class TimelineReader(FixDbContext dbContext) : ITimelineReader
{
    public async Task<IReadOnlyList<TimelineEntryDto>> ListAsync(
        Guid organizationId,
        string? entityType,
        Guid? entityId,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Timelines.AsNoTracking().Where(t => t.OrganizationId == organizationId);

        if (entityType is not null)
        {
            query = query.Where(t => t.EntityType == entityType);
        }

        if (entityId is not null)
        {
            query = query.Where(t => t.EntityId == entityId);
        }

        return await query
            .OrderByDescending(t => t.OccurredAt)
            .Take(limit)
            .Select(t => new TimelineEntryDto(t.Id, t.EntityType, t.EntityId, t.Action, t.Snapshot, t.AuthorId, t.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}
