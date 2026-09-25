/** Roles (permissões atômicas) do core. O front usa apenas para esconder/mostrar ações; quem autoriza é o core. */
export const Permission = {
  ViewPolicy: 'view_policy',
  CreatePolicy: 'create_policy',
  UpdatePolicy: 'update_policy',
  DeletePolicy: 'delete_policy',
  ApprovePolicy: 'approve_policy',
  ViewMandate: 'view_mandate',
  CreateMandate: 'create_mandate',
  UpdateMandate: 'update_mandate',
  DeleteMandate: 'delete_mandate',
  ApproveMandate: 'approve_mandate',
  ApproveException: 'approve_exception',
  ViewOrder: 'view_order',
  CreateOrder: 'create_order',
  UpdateOrder: 'update_order',
  DeleteOrder: 'delete_order',
  ApproveOrder: 'approve_order',
  ManageConfirmation: 'manage_confirmation',
  /** Alçada de emissão: mandatos dentro da política e boletas entram sem fila de aprovação. */
  SelfApprove: 'self_approve',
  ViewCounterparties: 'view_counterparties',
  ManageCounterparties: 'manage_counterparties',
  EditOrganization: 'edit_organization',
  ViewUsers: 'view_users',
} as const;

export type PermissionCode = (typeof Permission)[keyof typeof Permission];
