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
        var (page, pageSize) = request.Page.ToPaging();
        var result = await validation.RunAsync(
            new GetTimelineQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.HasEntityType ? request.EntityType : null,
                request.HasEntityId ? request.EntityId.ToOptionalGuid("entity_id") : null,
                request.Limit,
                Action: request.Action.ToOptionalString(request.HasAction),
                AuthorId: request.HasAuthorId ? request.AuthorId.ToOptionalGuid("author_id") : null,
                From: request.HasOccurredFrom ? request.OccurredFrom.ToOptionalMoment("occurred_from") : null,
                To: request.HasOccurredTo ? request.OccurredTo.ToOptionalMoment("occurred_to") : null,
                Search: request.Search.ToOptionalString(request.HasSearch),
                Page: page,
                PageSize: pageSize),
            timelineService.GetTimelineAsync,
            context.CancellationToken);

        var response = new GetTimelineResponse { Entries = { result.Entries.Select(e => e.ToContract()) } };
        if (result.TotalCount is { } total)
        {
            response.Page = new PageInfo { Page = result.Page, PageSize = result.PageSize, TotalCount = total };
        }

        return response;
    }
}
