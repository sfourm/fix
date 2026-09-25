using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Roles;

/// <summary>
/// Roles (permissões atômicas) fixas do sistema. Os ids são determinísticos para que o seed do banco
/// e as regras do domínio referenciem os mesmos registros.
/// </summary>
public static class SystemRoles
{
    public static readonly IReadOnlyDictionary<string, string> Descriptions = new Dictionary<string, string>
    {
        [RoleCodes.ViewPolicy] = "Visualizar a política de riscos",
        [RoleCodes.CreatePolicy] = "Criar política de riscos",
        [RoleCodes.UpdatePolicy] = "Editar política, eixos, bandas e instrumentos",
        [RoleCodes.DeletePolicy] = "Excluir política em rascunho",
        [RoleCodes.ApprovePolicy] = "Aprovar política (ata) e abrir novas versões",
        [RoleCodes.ViewMandate] = "Visualizar mandatos",
        [RoleCodes.CreateMandate] = "Emitir mandatos",
        [RoleCodes.UpdateMandate] = "Editar e encerrar mandatos",
        [RoleCodes.DeleteMandate] = "Excluir mandatos sem boletas",
        [RoleCodes.ApproveMandate] = "Aprovar mandatos dentro da política",
        [RoleCodes.ApproveException] = "Aprovar mandatos FORA da política (exceção)",
        [RoleCodes.ViewOrder] = "Visualizar boletas",
        [RoleCodes.CreateOrder] = "Registrar boletas",
        [RoleCodes.UpdateOrder] = "Editar boletas",
        [RoleCodes.DeleteOrder] = "Excluir boletas pendentes ou rejeitadas",
        [RoleCodes.ApproveOrder] = "Aprovar boletas",
        [RoleCodes.ManageConfirmation] = "Registrar e reconciliar confirmations (middle office)",
        [RoleCodes.SelfApprove] = "Alçada de emissão: operar sem passar pela fila de aprovação",
        [RoleCodes.ViewCounterparties] = "Visualizar contrapartes",
        [RoleCodes.ManageCounterparties] = "Cadastrar, homologar e limitar contrapartes",
        [RoleCodes.EditOrganization] = "Editar o setup da companhia, membros e grupos",
        [RoleCodes.ViewUsers] = "Visualizar membros e grupos",
    };

    public static readonly IReadOnlyList<string> All = [.. Descriptions.Keys];

    /// <summary>Somente leitura de todo o fluxo.</summary>
    public static readonly IReadOnlyList<string> ViewOnly =
    [
        RoleCodes.ViewPolicy,
        RoleCodes.ViewMandate,
        RoleCodes.ViewOrder,
        RoleCodes.ViewCounterparties,
    ];

    public static Guid Id(string code) => DeterministicGuid.From($"role:{code}");
}

