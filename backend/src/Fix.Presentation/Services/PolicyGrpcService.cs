using Fix.Application.Abstractions.Messaging;
using Fix.Application.Policies.Commands;
using Fix.Application.Policies.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class PolicyGrpcService(IDispatcher dispatcher) : PolicyService.PolicyServiceBase
{
    // ---------- Política ----------

    public override async Task<Policy> CreatePolicy(CreatePolicyRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
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
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicy(UpdatePolicyRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdatePolicyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Code,
                request.Title,
                request.Description.ToOptionalString(request.HasDescription),
                request.ValidFrom.ToDate("valid_from"),
                request.ValidTo.ToOptionalString(request.HasValidTo).ToOptionalDate("valid_to")),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicyLimits(UpdatePolicyLimitsRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdatePolicyLimitsCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Limits.ToDto()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> SubmitPolicy(PolicyIdRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new SubmitPolicyCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> ApprovePolicy(ApprovePolicyRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new ApprovePolicyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.ApprovalRecord),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> OpenPolicyVersion(OpenPolicyVersionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new OpenPolicyVersionCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Version,
                request.Reason.ToOptionalString(request.HasReason)),
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeletePolicy(PolicyIdRequest request, ServerCallContext context)
    {
        await dispatcher.SendAsync(
            new DeletePolicyCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Policy> GetPolicy(PolicyIdRequest request, ServerCallContext context) =>
        (await dispatcher.QueryAsync(
            new GetPolicyQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken)).ToContract();

    public override async Task<ListPoliciesResponse> ListPolicies(ListPoliciesRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var policies = await dispatcher.QueryAsync(
            new ListPoliciesQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), page, pageSize),
            context.CancellationToken);

        return new ListPoliciesResponse
        {
            Policies = { policies.Items.Select(p => p.ToContract()) },
            Page = policies.ToPageInfo(),
        };
    }

    // ---------- Eixos, bandas e instrumentos ----------

    public override async Task<Policy> AddPolicyAxis(AddPolicyAxisRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new AddPolicyAxisCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.Axis.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicyAxis(UpdatePolicyAxisRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdatePolicyAxisCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.AxisId.ToGuid("axis_id"),
                request.Axis.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> RemovePolicyAxis(PolicyChildRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RemovePolicyAxisCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.ChildId.ToGuid("child_id")),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> AddCoverageBand(AddCoverageBandRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new AddCoverageBandCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.Band.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdateCoverageBand(UpdateCoverageBandRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdateCoverageBandCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.BandId.ToGuid("band_id"),
                request.Band.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> RemoveCoverageBand(PolicyChildRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RemoveCoverageBandCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.ChildId.ToGuid("child_id")),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> AddPolicyInstrument(AddPolicyInstrumentRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new AddPolicyInstrumentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.Instrument.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> UpdatePolicyInstrument(UpdatePolicyInstrumentRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdatePolicyInstrumentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.InstrumentId.ToGuid("instrument_id"),
                request.Instrument.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Policy> RemovePolicyInstrument(PolicyChildRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RemovePolicyInstrumentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.PolicyId.ToGuid("policy_id"),
                request.ChildId.ToGuid("child_id")),
            context.CancellationToken)).ToContract();
}
