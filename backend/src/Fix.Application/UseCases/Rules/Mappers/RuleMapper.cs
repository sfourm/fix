using Fix.Application.Rules.Dtos;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Application.Rules.Mappers;

internal static class RuleMapper
{
    /// <summary>Códigos das roles pelos ids determinísticos do catálogo fixo de roles.</summary>
    private static readonly IReadOnlyDictionary<Guid, string> RoleCodes = SystemRoles.All.ToDictionary(SystemRoles.Id);

    public static RuleDto ToDto(this Rule rule, int usages) => new(
        rule.Id,
        rule.Code,
        rule.Name,
        rule.Roles.Select(r => RoleCodes.GetValueOrDefault(r.RoleId, r.RoleId.ToString())).Order().ToList(),
        rule.IsSystem,
        usages);
}
