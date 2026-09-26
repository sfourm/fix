namespace Fix.Domain.AggregateRoots.Roles;

/// <summary>Códigos das roles (permissões atômicas). As alçadas do FIX são roles agrupadas em rules.</summary>
public static class RoleCodes
{
    // Política de riscos
    public const string ViewPolicy = "view_policy";
    public const string CreatePolicy = "create_policy";
    public const string UpdatePolicy = "update_policy";
    public const string DeletePolicy = "delete_policy";
    public const string ApprovePolicy = "approve_policy";

    // Mandatos
    public const string ViewMandate = "view_mandate";
    public const string CreateMandate = "create_mandate";
    public const string UpdateMandate = "update_mandate";
    public const string DeleteMandate = "delete_mandate";
    public const string ApproveMandate = "approve_mandate";
    public const string ApproveException = "approve_exception";

    // Boletas (orders)
    public const string ViewOrder = "view_order";
    public const string CreateOrder = "create_order";
    public const string UpdateOrder = "update_order";
    public const string DeleteOrder = "delete_order";
    public const string ApproveOrder = "approve_order";
    public const string ManageConfirmation = "manage_confirmation";

    /// <summary>Alçada de emissão: mandatos dentro da política e boletas entram direto, sem fila de aprovação.</summary>
    public const string SelfApprove = "self_approve";

    // Setup
    public const string ViewCounterparties = "view_counterparties";
    public const string ManageCounterparties = "manage_counterparties";
    /// <summary>Setup da companhia: identificação, capacidade, orçamento, financeiro e commodities.</summary>
    public const string EditOrganization = "edit_organization";

    // Usuários (membros, grupos/organograma e cargos)
    public const string ViewUser = "view_user";
    public const string CreateUser = "create_user";
    public const string UpdateUser = "update_user";
    public const string DeleteUser = "delete_user";
}

