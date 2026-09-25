using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Policies.Commands;
using Fix.Application.Policies.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class PolicyGrpcService(
    IValidationFactory validation,
    IPolicyService policyService) : PolicyService.PolicyServiceBase
{
    // ---------- Política ----------

    public override async Task<Policy> CreatePolicy(CreatePolicyRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new CreatePolicyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Code,
                request.Title,
                request.Version,
                request.Description.ToOptionalString(request.HasDescription),
                request.ValidFrom.ToDate("valid_from"),
                request.ValidTo.ToOptionalString(request.HasValidTo).ToOptionalDate("valid_to"),
                request.UseTemplate),
            policyService.CreatePolicyAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicy(UpdatePolicyRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdatePolicyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Code,
                request.Title,
                request.Description.ToOptionalString(request.HasDescription),
                request.ValidFrom.ToDate("valid_from"),
                request.ValidTo.ToOptionalString(request.HasValidTo).ToOptionalDate("valid_to")),
            policyService.UpdatePolicyAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicyLimits(UpdatePolicyLimitsRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdatePolicyLimitsCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Limits.ToDto()),
            policyService.UpdatePolicyLimitsAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> SubmitPolicy(PolicyIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new SubmitPolicyCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            policyService.SubmitPolicyAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> ApprovePolicy(ApprovePolicyRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new ApprovePolicyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.ApprovalRecord),
            policyService.ApprovePolicyAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> OpenPolicyVersion(OpenPolicyVersionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new OpenPolicyVersionCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Version,
                request.Reason.ToOptionalString(request.HasReason)),
            policyService.OpenPolicyVersionAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeletePolicy(PolicyIdRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new DeletePolicyCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            policyService.DeletePolicyAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Policy> GetPolicy(PolicyIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetPolicyQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            policyService.GetPolicyAsync,
            context.CancellationToken)).ToContract();

    public override async Task<ListPoliciesResponse> ListPolicies(ListPoliciesRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var policies = await validation.RunAsync(
            new ListPoliciesQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), page, pageSize),
            policyService.ListPoliciesAsync,
            context.CancellationToken);

        return new ListPoliciesResponse
        {
            Policies = { policies.Items.Select(p => p.ToContract()) },
            Page = policies.ToPageInfo(),
        };
    }

    // ---------- Eixos, bandas e instrumentos ----------

    public override async Task<Policy> AddPolicyAxis(AddPolicyAxisRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new AddPolicyAxisCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.Axis.ToInput()),
            policyService.AddPolicyAxisAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicyAxis(UpdatePolicyAxisRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdatePolicyAxisCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.AxisId.ToGuid("axis_id"),
                request.Axis.ToInput()),
            policyService.UpdatePolicyAxisAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> RemovePolicyAxis(PolicyChildRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RemovePolicyAxisCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.ChildId.ToGuid("child_id")),
            policyService.RemovePolicyAxisAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> AddCoverageBand(AddCoverageBandRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new AddCoverageBandCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.Band.ToInput()),
            policyService.AddCoverageBandAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdateCoverageBand(UpdateCoverageBandRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdateCoverageBandCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.BandId.ToGuid("band_id"),
                request.Band.ToInput()),
            policyService.UpdateCoverageBandAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> RemoveCoverageBand(PolicyChildRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RemoveCoverageBandCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.ChildId.ToGuid("child_id")),
            policyService.RemoveCoverageBandAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> AddPolicyInstrument(AddPolicyInstrumentRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new AddPolicyInstrumentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.Instrument.ToInput()),
            policyService.AddPolicyInstrumentAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicyInstrument(UpdatePolicyInstrumentRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdatePolicyInstrumentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.InstrumentId.ToGuid("instrument_id"),
                request.Instrument.ToInput()),
            policyService.UpdatePolicyInstrumentAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Policy> RemovePolicyInstrument(PolicyChildRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RemovePolicyInstrumentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.ChildId.ToGuid("child_id")),
            policyService.RemovePolicyInstrumentAsync,
            context.CancellationToken)).ToContract();
}
