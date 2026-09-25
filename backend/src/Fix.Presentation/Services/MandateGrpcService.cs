using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Entities = Fix.Domain.AggregateRoots.Mandates;

namespace Fix.Presentation.Services;

internal sealed class MandateGrpcService(
    IValidationFactory validation,
    IMandateService mandateService) : MandateService.MandateServiceBase
{
    public override async Task<Mandate> IssueMandate(IssueMandateRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new IssueMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.AxisId.ToGuid("axis_id"),
                request.Type.ToDomain<Entities.MandateType>("type"),
                request.Terms.ToInput()),
            mandateService.IssueMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> UpdateMandate(UpdateMandateRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdateMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Terms.ToInput()),
            mandateService.UpdateMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> ApproveMandate(MandateDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new ApproveMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote)),
            mandateService.ApproveMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> RejectMandate(MandateDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RejectMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            mandateService.RejectMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Mandate> CloseMandate(MandateDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new CloseMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote)),
            mandateService.CloseMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeleteMandate(MandateIdRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new DeleteMandateCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            mandateService.DeleteMandateAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Mandate> GetMandate(MandateIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetMandateQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            mandateService.GetMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<ListMandatesResponse> ListMandates(ListMandatesRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var mandates = await validation.RunAsync(
            new ListMandatesQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToOptionalString(request.HasPolicyId).ToOptionalGuid("policy_id"),
                request.Status.ToOptionalDomain<Entities.MandateStatus>("status"),
                page,
                pageSize),
            mandateService.ListMandatesAsync,
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
        (await validation.RunAsync(
            new PreviewMandateComplianceQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.AxisId.ToGuid("axis_id"),
                request.Type.ToDomain<Entities.MandateType>("type"),
                request.Terms.ToInput()),
            mandateService.PreviewMandateComplianceAsync,
            context.CancellationToken)).ToContract();
}

