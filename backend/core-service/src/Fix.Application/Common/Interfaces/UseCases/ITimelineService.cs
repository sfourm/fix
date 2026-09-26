using Fix.Application.Abstractions.Timeline;
using Fix.Application.Timeline.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Auditoria das entidades. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface ITimelineService
{
    Task<TimelinePage> GetTimelineAsync(GetTimelineQuery query, CancellationToken cancellationToken);
}
