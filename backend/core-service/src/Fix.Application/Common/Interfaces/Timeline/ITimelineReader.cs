namespace Fix.Application.Abstractions.Timeline;

public sealed record TimelineEntryDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string Snapshot,
    Guid? AuthorId,
    DateTimeOffset OccurredAt);

/// <summary>Leitura da trilha de auditoria gravada pelo interceptor do EF Core.</summary>
public interface ITimelineReader
{
    Task<IReadOnlyList<TimelineEntryDto>> ListAsync(
        Guid organizationId,
        string? entityType,
        Guid? entityId,
        int limit,
        CancellationToken cancellationToken);
}
