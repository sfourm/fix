using Fix.Application.Abstractions.Timeline;
using Fix.Application.Timeline.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Timeline de auditoria das entidades. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface ITimelineService
{
    Task<IReadOnlyList<TimelineEntryDto>> GetTimelineAsync(GetTimelineQuery query, CancellationToken cancellationToken);
}
