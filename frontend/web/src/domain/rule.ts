import { Permission, type PermissionCode } from './permissions';

/**
 * Rule: conjunto de roles. As de sistema (owner, user e as internas da FIX) são fixas; as alçadas personalizadas
 * são criadas e editadas pela organização e atribuídas a membros e grupos.
 */
export interface Rule {
  id: string;
  code: string;
  name: string;
  /** Códigos das roles concedidas. */
  roles: string[];
  isSystem: boolean;
  /** Membros e grupos que usam a rule na organização. */
  usages: number;
}

/** Papéis internos da equipe FIX: nunca são exibidos nem atribuídos em organizações clientes. */
export const INTERNAL_RULES = ['super_administrador', 'administrador'] as const;

export const isInternalRule = (code: string) => (INTERNAL_RULES as readonly string[]).includes(code);

/** Base de todo membro (não é uma alçada). */
export const baseRoleLabel: Record<string, string> = {
  owner: 'Owner',
  user: 'Usuário',
  super_administrador: 'Super administrador (FIX)',
  administrador: 'Administrador (FIX)',
};

/** Roles agrupadas por área, para montar alçadas; as de decisão ficam marcadas (a equipe FIX nunca as tem). */
export const ROLE_GROUPS: { title: string; roles: PermissionCode[] }[] = [
  {
    title: 'Política de riscos',
    roles: [Permission.ViewPolicy, Permission.CreatePolicy, Permission.UpdatePolicy, Permission.DeletePolicy, Permission.ApprovePolicy],
  },
  {
    title: 'Mandatos',
    roles: [
      Permission.ViewMandate,
      Permission.CreateMandate,
      Permission.UpdateMandate,
      Permission.DeleteMandate,
      Permission.ApproveMandate,
      Permission.ApproveException,
    ],
  },
  {
    title: 'Boletas',
    roles: [
      Permission.ViewOrder,
      Permission.CreateOrder,
      Permission.UpdateOrder,
      Permission.DeleteOrder,
      Permission.ApproveOrder,
      Permission.ManageConfirmation,
    ],
  },
  { title: 'Alçada de emissão', roles: [Permission.SelfApprove] },
  { title: 'Contrapartes', roles: [Permission.ViewCounterparties, Permission.ManageCounterparties] },
  { title: 'Organização', roles: [Permission.EditOrganization] },
  { title: 'Usuários, grupos e cargos', roles: [Permission.ViewUser, Permission.CreateUser, Permission.UpdateUser, Permission.DeleteUser] },
];

export const DECISION_ROLES: PermissionCode[] = [
  Permission.ApprovePolicy,
  Permission.ApproveMandate,
  Permission.ApproveException,
  Permission.ApproveOrder,
  Permission.ManageConfirmation,
  Permission.SelfApprove,
];
