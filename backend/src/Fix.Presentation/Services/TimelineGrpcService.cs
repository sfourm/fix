using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Timeline.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class TimelineGrpcService(
    IValidationFactory validation,
    ITimelineService timelineService) : TimelineService.TimelineServiceBase
{
    public override async Task<GetTimelineResponse> GetTimeline(GetTimelineRequest request, ServerCallContext context)
    {
        var entries = await validation.RunAsync(
            new GetTimelineQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.HasEntityType ? request.EntityType : null,
                request.HasEntityId ? request.EntityId.ToOptionalGuid("entity_id") : null,
                request.Limit),
            timelineService.GetTimelineAsync,
            context.CancellationToken);

        return new GetTimelineResponse { Entries = { entries.Select(e => e.ToContract()) } };
    }
}
