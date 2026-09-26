namespace Fix.Application.Abstractions.Timeline;

public sealed record TimelineEntryDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string Snapshot,
    Guid? AuthorId,
    DateTimeOffset OccurredAt);

/// <summary>Filtros da auditoria (todos opcionais; <see cref="To"/> é exclusivo). <see cref="Skip"/>/<see cref="Take"/> recortam a página.</summary>
public sealed record TimelineFilter(
    string? EntityType,
    Guid? EntityId,
    string? Action,
    Guid? AuthorId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Search,
    int Skip,
    int Take,
    bool CountTotal);

/// <summary>Registros da página e, quando pedido, o total que atende aos filtros.</summary>
public sealed record TimelinePage(IReadOnlyList<TimelineEntryDto> Entries, int? TotalCount, int Page, int PageSize);

/// <summary>Leitura da trilha de auditoria gravada pelo interceptor do EF Core.</summary>
public interface ITimelineReader
{
    Task<(IReadOnlyList<TimelineEntryDto> Entries, int? TotalCount)> ListAsync(
        Guid organizationId,
        TimelineFilter filter,
        CancellationToken cancellationToken);
}
