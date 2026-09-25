using Fix.Application.Abstractions.Messaging;
using Fix.Application.Timeline.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class TimelineGrpcService(IDispatcher dispatcher) : TimelineService.TimelineServiceBase
{
    public override async Task<GetTimelineResponse> GetTimeline(GetTimelineRequest request, ServerCallContext context)
    {
        var entries = await dispatcher.QueryAsync(
            new GetTimelineQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.HasEntityType ? request.EntityType : null,
                request.HasEntityId ? request.EntityId.ToOptionalGuid("entity_id") : null,
                request.Limit),
            context.CancellationToken);

        return new GetTimelineResponse { Entries = { entries.Select(e => e.ToContract()) } };
    }
}
