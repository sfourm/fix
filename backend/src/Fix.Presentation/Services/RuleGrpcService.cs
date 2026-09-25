using Fix.Application.Abstractions.Messaging;
using Fix.Application.Rules.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class RuleGrpcService(IDispatcher dispatcher) : RuleService.RuleServiceBase
{
    public override async Task<ListRulesResponse> ListRules(Empty request, ServerCallContext context)
    {
        var rules = await dispatcher.QueryAsync(new ListRulesQuery(), context.CancellationToken);
        return new ListRulesResponse { Rules = { rules.Select(r => r.ToContract()) } };
    }
}
