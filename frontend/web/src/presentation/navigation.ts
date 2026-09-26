import { Permission, type PermissionCode } from '@/domain/permissions';

/** Subitem de uma área (aparece no menu quando a área está aberta). */
export interface NavChild {
  to: string;
  label: string;
  permission?: PermissionCode;
  match: RegExp;
}

export interface Area {
  key: 'home' | 'policies' | 'users' | 'organization' | 'uploads';
  label: string;
  /** Legenda curta abaixo do nome no menu. */
  hint: string;
  to: string;
  match: RegExp;
  permission?: PermissionCode;
  children: NavChild[];
}

/**
 * Menu lateral: Home e quatro áreas. Políticas é a raiz da cadeia 1:N (política → mandato → boleta): mandatos, boletas,
 * aprovações e confirmações só aparecem dentro de uma política — o menu mostra o caminho aberto como trilha.
 * Usuários e Organização mostram os subitens quando abertas; Uploads mostra o tipo e o arquivo abertos como trilha.
 */
export const AREAS: Area[] = [
  { key: 'home', label: 'Home', hint: 'dashboards', to: '/', match: /^\/$/, children: [] },
  {
    key: 'policies',
    label: 'Políticas',
    hint: 'mandatos · boletas · exceções',
    to: '/policies',
    match: /^\/(policies|exceptions)(\/|$)/,
    permission: Permission.ViewPolicy,
    children: [
      { to: '/policies', label: 'Políticas', match: /^\/policies(\/|$)/ },
      // Boletas sem mandato ou FORA do enquadramento (FIX2 · I-01: desvio exposto, nunca silencioso).
      { to: '/exceptions', label: 'Exceções', permission: Permission.ViewOrder, match: /^\/exceptions(\/|$)/ },
    ],
  },
  {
    key: 'users',
    label: 'Usuários',
    hint: 'membros · grupos · cargos',
    to: '/members',
    match: /^\/(members|access)(\/|$)/,
    children: [
      { to: '/members', label: 'Membros e grupos', permission: Permission.ViewUser, match: /^\/members(\/|$)/ },
      { to: '/access', label: 'Cargos e Regras', permission: Permission.ViewUser, match: /^\/access$/ },
    ],
  },
  {
    key: 'uploads',
    label: 'Uploads',
    hint: 'planilhas · documentos',
    to: '/uploads',
    match: /^\/uploads(\/|$)/,
    children: [],
  },
  {
    key: 'organization',
    label: 'Organização',
    hint: 'setup · contrapartes · auditoria',
    to: '/setup',
    match: /^\/(setup|counterparties|audit)$/,
    children: [
      { to: '/setup', label: 'Setup da companhia', match: /^\/setup$/ },
      { to: '/counterparties', label: 'Contrapartes', permission: Permission.ViewCounterparties, match: /^\/counterparties$/ },
      { to: '/audit', label: 'Auditoria', match: /^\/audit$/ },
    ],
  },
];
