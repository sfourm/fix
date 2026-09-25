using Fix.Application.Rules.Commands;
using Fix.Application.Rules.Dtos;
using Fix.Application.Rules.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Rules e alçadas personalizadas da organização. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IRuleService
{
    Task<IReadOnlyList<RuleDto>> ListRulesAsync(ListRulesQuery query, CancellationToken cancellationToken);

    Task<RuleDto> CreateRuleAsync(CreateRuleCommand command, CancellationToken cancellationToken);

    Task<RuleDto> UpdateRuleAsync(UpdateRuleCommand command, CancellationToken cancellationToken);

    Task DeleteRuleAsync(DeleteRuleCommand command, CancellationToken cancellationToken);
}
