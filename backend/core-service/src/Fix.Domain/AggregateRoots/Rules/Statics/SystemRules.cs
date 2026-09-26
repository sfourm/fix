using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Domain.AggregateRoots.Rules;

/// <summary>
/// Rules de sistema (fixas, com ids determinísticos para o seed) e os modelos de alçada que cada nova
/// organização recebe como ponto de partida. As alçadas da organização são rules dela, editáveis.
/// </summary>
public static class SystemRules
{
    public static readonly IReadOnlyList<Definition> All =
    [
        new(RuleCodes.SuperAdministrador, "Super administrador (FIX)", SystemRoles.All, Internal: true),
        new(RuleCodes.Administrador, "Administrador (FIX)", [RoleCodes.ViewUser], Internal: true),
        new(RuleCodes.Owner, "Owner", SystemRoles.All),
        new(RuleCodes.User, "Usuário", SystemRoles.ViewOnly),
    ];

    /// <summary>Alçadas iniciais de toda organização cliente (copiadas para ela, que pode editar ou excluir).</summary>
    public static readonly IReadOnlyList<Definition> Templates =
    [
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
            RoleCodes.ViewUser,
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
    ];

    public static readonly Guid OwnerId = Id(RuleCodes.Owner);
    public static readonly Guid UserId = Id(RuleCodes.User);
    public static readonly Guid SuperAdministradorId = Id(RuleCodes.SuperAdministrador);
    public static readonly Guid AdministradorId = Id(RuleCodes.Administrador);

    /// <summary>Rules de base: definem o papel do membro (owner/user ou equipe interna), não uma alçada.</summary>
    public static readonly IReadOnlySet<Guid> BaseIds = new HashSet<Guid> { OwnerId, UserId, SuperAdministradorId, AdministradorId };

    public static bool IsInternal(string code) => code is RuleCodes.SuperAdministrador or RuleCodes.Administrador;

    public static Guid Id(string code) => DeterministicGuid.From($"rule:{code}");

    public static Guid RuleRoleId(string ruleCode, string roleCode) =>
        DeterministicGuid.From($"rule-role:{ruleCode}:{roleCode}");

    public sealed record Definition(string Code, string Name, IReadOnlyList<string> Roles, bool Internal = false);
}
