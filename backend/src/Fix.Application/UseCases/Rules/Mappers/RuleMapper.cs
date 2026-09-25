using Fix.Application.Rules.Dtos;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Application.Rules.Mappers;

internal static class RuleMapper
{
    public static RuleDto ToDto(this Rule rule, IReadOnlyDictionary<Guid, string> roleCodes) => new(
        rule.Id,
        rule.Code,
        rule.Name,
        rule.Roles.Select(r => roleCodes[r.RoleId]).Order().ToList());
}

