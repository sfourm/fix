using Fix.Application.Rules.Dtos;
using Fix.Contracts.V1;

namespace Fix.Presentation.Mappers;

internal static class RuleContractMapper
{
    public static Rule ToContract(this RuleDto rule) => new()
    {
        Id = rule.Id.ToString(),
        Code = rule.Code,
        Name = rule.Name,
        Roles = { rule.Roles },
        IsSystem = rule.IsSystem,
        Usages = rule.Usages,
    };
}
