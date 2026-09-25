using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Domain.AggregateRoots.Rules;

/// <summary>
/// Rules fixas do sistema e as roles (alçadas) de cada uma. Os ids são determinísticos para que o seed
/// do banco e as regras do domínio (ex.: founder ao criar organização) referenciem os mesmos registros.
/// </summary>
public static class SystemRules
{
    public static readonly IReadOnlyList<Definition> All =
    [
        new(RuleCodes.SuperAdministrador, "Super administrador", SystemRoles.All),
        new(RuleCodes.Administrador, "Administrador", SystemRoles.All),
        new(RuleCodes.Founder, "Founder", SystemRoles.All),
        new(RuleCodes.Gestor, "Gestor (Diretoria)",
        [
            .. SystemRoles.ViewOnly,
            RoleCodes.UpdatePolicy,
            RoleCodes.ApprovePolicy,
            RoleCodes.CreateMandate,
            RoleCodes.UpdateMandate,
            RoleCodes.ApproveMandate,
            RoleCodes.ApproveException,
            RoleCodes.CreateOrder,
            RoleCodes.UpdateOrder,
            RoleCodes.ApproveOrder,
            RoleCodes.SelfApprove,
            RoleCodes.ViewUsers,
        ]),
        new(RuleCodes.Operador, "Operador (Mesa)",
        [
            .. SystemRoles.ViewOnly,
            RoleCodes.CreateMandate,
            RoleCodes.CreateOrder,
            RoleCodes.UpdateOrder,
        ]),
        new(RuleCodes.MiddleOffice, "Middle office (Controle de riscos)",
        [
            .. SystemRoles.ViewOnly,
            RoleCodes.ManageConfirmation,
        ]),
        new(RuleCodes.User, "Usuário (leitura)", SystemRoles.ViewOnly),
    ];

    public static Guid Id(string code) => DeterministicGuid.From($"rule:{code}");

    public static Guid RuleRoleId(string ruleCode, string roleCode) =>
        DeterministicGuid.From($"rule-role:{ruleCode}:{roleCode}");

    public sealed record Definition(string Code, string Name, IReadOnlyList<string> Roles);
}

