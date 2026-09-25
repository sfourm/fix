using Fix.Application.Abstractions.Messaging;
using Fix.Application.Rules.Dtos;
using Fix.Application.Rules.Mappers;
using Fix.Application.Rules.Queries;
using Fix.Domain.AggregateRoots.Roles.Repositories;
using Fix.Domain.AggregateRoots.Rules.Repositories;

namespace Fix.Application.Rules.Services;

internal sealed class RuleService(IRuleRepository ruleRepository, IRoleRepository roleRepository)
    : IQueryHandler<ListRulesQuery, IReadOnlyList<RuleDto>>
{
    public async Task<IReadOnlyList<RuleDto>> HandleAsync(ListRulesQuery query, CancellationToken cancellationToken)
    {
        var roleCodes = (await roleRepository.ListAsync(cancellationToken)).ToDictionary(r => r.Id, r => r.Code);
        var rules = await ruleRepository.ListAsync(cancellationToken);
        return rules.Select(r => r.ToDto(roleCodes)).ToList();
    }
}
