using Fix.Application.Abstractions.Messaging;
using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Entities = Fix.Domain.AggregateRoots.Mandates;

namespace Fix.Presentation.Services;

internal sealed class MandateGrpcService(IDispatcher dispatcher) : MandateService.MandateServiceBase
{
    public override async Task<Mandate> IssueMandate(IssueMandateRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new IssueMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.AxisId.ToGuid("axis_id"),
                request.Type.ToDomain<Entities.MandateType>("type"),
                request.Terms.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> UpdateMandate(UpdateMandateRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdateMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Terms.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> ApproveMandate(MandateDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new ApproveMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote)),
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> RejectMandate(MandateDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RejectMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> CloseMandate(MandateDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new CloseMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote)),
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeleteMandate(MandateIdRequest request, ServerCallContext context)
    {
        await dispatcher.SendAsync(
            new DeleteMandateCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Mandate> GetMandate(MandateIdRequest request, ServerCallContext context) =>
        (await dispatcher.QueryAsync(
            new GetMandateQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken)).ToContract();

    public override async Task<ListMandatesResponse> ListMandates(ListMandatesRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var mandates = await dispatcher.QueryAsync(
            new ListMandatesQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToOptionalString(request.HasPolicyId).ToOptionalGuid("policy_id"),
                request.Status.ToOptionalDomain<Entities.MandateStatus>("status"),
                page,
                pageSize),
            context.CancellationToken);

        return new ListMandatesResponse
        {
            Mandates = { mandates.Items.Select(m => m.ToContract()) },
            Page = mandates.ToPageInfo(),
        };
    }

    public override async Task<Compliance> PreviewMandateCompliance(
        PreviewMandateComplianceRequest request,
        ServerCallContext context) =>
        (await dispatcher.QueryAsync(
            new PreviewMandateComplianceQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.AxisId.ToGuid("axis_id"),
                request.Type.ToDomain<Entities.MandateType>("type"),
                request.Terms.ToInput()),
            context.CancellationToken)).ToContract();
}

