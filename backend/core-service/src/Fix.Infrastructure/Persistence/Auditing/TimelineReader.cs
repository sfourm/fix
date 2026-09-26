using Fix.Application.Abstractions.Timeline;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Auditing;

internal sealed class TimelineReader(FixDbContext dbContext) : ITimelineReader
{
    public async Task<(IReadOnlyList<TimelineEntryDto> Entries, int? TotalCount)> ListAsync(
        Guid organizationId,
        TimelineFilter filter,
        CancellationToken cancellationToken)
    {
        // Busca textual no snapshot (jsonb) e no id: o jsonb precisa virar texto, então parte de um SQL que o EF compõe.
        var query = filter.Search is null
            ? dbContext.Timelines.AsNoTracking()
            : dbContext.Timelines
                .FromSqlInterpolated($"SELECT * FROM timelines WHERE snapshot::text ILIKE {Like(filter.Search)} OR entity_id::text ILIKE {Like(filter.Search)}")
                .AsNoTracking();

        query = query.Where(t => t.OrganizationId == organizationId);

        if (filter.EntityType is not null)
        {
            query = query.Where(t => t.EntityType == filter.EntityType);
        }

        if (filter.EntityId is not null)
        {
            query = query.Where(t => t.EntityId == filter.EntityId);
        }

        if (filter.Action is not null)
        {
            query = query.Where(t => t.Action == filter.Action);
        }

        if (filter.AuthorId is not null)
        {
            query = query.Where(t => t.AuthorId == filter.AuthorId);
        }

        if (filter.From is not null)
        {
            query = query.Where(t => t.OccurredAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(t => t.OccurredAt < filter.To);
        }

        int? total = filter.CountTotal ? await query.CountAsync(cancellationToken) : null;

        var entries = await query
            .OrderByDescending(t => t.OccurredAt)
            .ThenByDescending(t => t.Id)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .Select(t => new TimelineEntryDto(t.Id, t.EntityType, t.EntityId, t.Action, t.Snapshot, t.AuthorId, t.OccurredAt))
            .ToListAsync(cancellationToken);

        return (entries, total);
    }

    /// <summary>Padrão ILIKE "contém", com %, _ e \ do texto tratados como literais.</summary>
    private static string Like(string text) =>
        $"%{text.Replace(@"\", @"\\", StringComparison.Ordinal).Replace("%", @"\%", StringComparison.Ordinal).Replace("_", @"\_", StringComparison.Ordinal)}%";
}
