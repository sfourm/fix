using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Rules.Commands;
using Fix.Application.Rules.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class RuleGrpcService(
    IValidationFactory validation,
    IRuleService ruleService) : RuleService.RuleServiceBase
{
    public override async Task<ListRulesResponse> ListRules(ListRulesRequest request, ServerCallContext context)
    {
        var rules = await validation.RunAsync(
            new ListRulesQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            ruleService.ListRulesAsync,
            context.CancellationToken);

        return new ListRulesResponse { Rules = { rules.Select(r => r.ToContract()) } };
    }

    public override async Task<Rule> CreateRule(CreateRuleRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new CreateRuleCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Name,
                request.RoleCodes.ToList()),
            ruleService.CreateRuleAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Rule> UpdateRule(UpdateRuleRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdateRuleCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Name,
                request.RoleCodes.ToList()),
            ruleService.UpdateRuleAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeleteRule(RuleIdRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new DeleteRuleCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            ruleService.DeleteRuleAsync,
            context.CancellationToken);

        return new Empty();
    }
}
