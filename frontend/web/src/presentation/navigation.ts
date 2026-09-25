import { Permission, type PermissionCode } from '@/domain/permissions';

/** Subitem de uma área (aparece no menu quando a área está aberta). */
export interface NavChild {
  to: string;
  label: string;
  permission?: PermissionCode;
  match: RegExp;
}

export interface Area {
  key: 'home' | 'policies' | 'users' | 'organization';
  label: string;
  /** Legenda curta abaixo do nome no menu. */
  hint: string;
  to: string;
  match: RegExp;
  permission?: PermissionCode;
  children: NavChild[];
}

/**
 * Menu lateral: Home e três áreas. Políticas é a raiz da cadeia 1:N (política → mandato → boleta): mandatos, boletas,
 * aprovações e confirmations só aparecem dentro de uma política — o menu mostra o caminho aberto como trilha.
 * Usuários e Organização mostram os subitens quando abertas.
 */
export const AREAS: Area[] = [
  { key: 'home', label: 'Home', hint: 'dashboards', to: '/', match: /^\/$/, children: [] },
  {
    key: 'policies',
    label: 'Políticas',
    hint: 'mandatos · boletas',
    to: '/policies',
    match: /^\/policies(\/|$)/,
    permission: Permission.ViewPolicy,
    children: [],
  },
  {
    key: 'users',
    label: 'Usuários',
    hint: 'membros · grupos · cargos',
    to: '/members',
    match: /^\/(members|access)(\/|$)/,
    children: [
      { to: '/members', label: 'Membros e grupos', permission: Permission.ViewUsers, match: /^\/members(\/|$)/ },
      { to: '/access', label: 'Cargos e Regras', match: /^\/access$/ },
    ],
  },
  {
    key: 'organization',
    label: 'Organização',
    hint: 'setup · contrapartes',
    to: '/setup',
    match: /^\/(setup|counterparties|timeline)$/,
    children: [
      { to: '/setup', label: 'Setup da companhia', match: /^\/setup$/ },
      { to: '/counterparties', label: 'Contrapartes', permission: Permission.ViewCounterparties, match: /^\/counterparties$/ },
      { to: '/timeline', label: 'Timeline', match: /^\/timeline$/ },
    ],
  },
];
