import { Permission, type PermissionCode } from '@/domain/permissions';

/** Aba de uma área: uma tela, com a role necessária para vê-la e, opcionalmente, um contador. */
export interface AreaTab {
  to: string;
  label: string;
  permission?: PermissionCode;
  counter?: 'approvals' | 'confirmations';
  /** Rotas que também ativam a aba (ex.: detalhes e formulários). */
  match?: RegExp;
}

export interface Area {
  key: 'home' | 'policies' | 'users' | 'organization';
  label: string;
  tabs: AreaTab[];
}

/**
 * Menu lateral: Home (dashboards) e três áreas cujas telas viram abas no topo do conteúdo.
 * Políticas segue a cadeia política → mandato → boleta; Usuários reúne membros, organograma e alçadas;
 * Organização reúne setup, contrapartes e auditoria.
 */
export const AREAS: Area[] = [
  { key: 'home', label: 'Home', tabs: [{ to: '/', label: 'Home', match: /^\/$/ }] },
  {
    key: 'policies',
    label: 'Políticas',
    tabs: [
      { to: '/policies', label: 'Política de riscos', permission: Permission.ViewPolicy, match: /^\/policies(\/|$)/ },
      { to: '/mandates', label: 'Mandatos', permission: Permission.ViewMandate, match: /^\/mandates(\/|$)/ },
      { to: '/orders', label: 'Boletas de hedge', permission: Permission.ViewOrder, match: /^\/orders(\/(?!open$)|$)/ },
      { to: '/orders/open', label: 'Boletas em aberto', permission: Permission.ViewOrder, counter: 'confirmations', match: /^\/orders\/open$/ },
      { to: '/approvals', label: 'Fila de aprovação', counter: 'approvals', match: /^\/approvals$/ },
    ],
  },
  {
    key: 'users',
    label: 'Usuários',
    tabs: [
      { to: '/members', label: 'Membros e grupos', permission: Permission.ViewUsers, match: /^\/members$/ },
      { to: '/access', label: 'Rules e alçadas', match: /^\/access$/ },
    ],
  },
  {
    key: 'organization',
    label: 'Organização',
    tabs: [
      { to: '/setup', label: 'Setup da companhia', match: /^\/setup$/ },
      { to: '/counterparties', label: 'Contrapartes', permission: Permission.ViewCounterparties, match: /^\/counterparties$/ },
      { to: '/timeline', label: 'Timeline', match: /^\/timeline$/ },
    ],
  },
];

export const isTabActive = (tab: AreaTab, path: string) => (tab.match ? tab.match.test(path) : path === tab.to);
