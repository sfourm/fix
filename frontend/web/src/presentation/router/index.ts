import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useSessionStore } from '@/application/stores/session.store';
import { Permission, type PermissionCode } from '@/domain/permissions';
import { paths } from '../paths';
import type { Crumb, TrailStore } from '../trail';

declare module 'vue-router' {
  interface RouteMeta {
    /** Rota pública (login/cadastro); usuário logado é redirecionado. */
    guest?: boolean;
    /** Exige organização selecionada (tenant). */
    organization?: boolean;
    permission?: PermissionCode;
  }
}

type Trail = (p: Record<string, string>, t: TrailStore) => Crumb[];
const page = (permission?: PermissionCode, trail?: Trail) => ({ organization: true, permission, trail });

// ---------- Breadcrumbs da cadeia 1:N ----------
const policiesCrumb: Crumb = { label: 'Políticas', to: paths.policies() };
const policyCrumbs: Trail = (p, t) => [policiesCrumb, { label: t.get(p.policyId), to: paths.policy(p.policyId!) }];
const mandateCrumbs: Trail = (p, t) => [...policyCrumbs(p, t), { label: t.get(p.mandateId), to: paths.mandate(p.policyId!, p.mandateId!) }];
const area = (label: string, current: string): Trail => () => [{ label }, { label: current }];

const routes: RouteRecordRaw[] = [
  // AppLayout primeiro: com dois pais em '/', o primeiro definido vence para a rota raiz.
  {
    path: '/',
    component: () => import('../layouts/AppLayout.vue'),
    meta: { organization: true },
    children: [
      { path: '', component: () => import('../views/home/HomeView.vue'), meta: page(undefined, () => [{ label: 'Home' }]) },

      // ---------- Políticas → mandatos → boletas (sempre a partir da política) ----------
      {
        path: 'policies',
        component: () => import('../views/policies/PolicyListView.vue'),
        meta: page(Permission.ViewPolicy, () => [{ label: 'Políticas' }]),
      },
      {
        path: 'policies/:policyId',
        component: () => import('../views/policies/PolicyShellView.vue'),
        props: true,
        meta: page(Permission.ViewPolicy, policyCrumbs),
        children: [
          { path: '', component: () => import('../views/policies/tabs/PolicyOverviewTab.vue') },
          { path: 'axes', component: () => import('../views/policies/tabs/PolicyAxesTab.vue') },
          { path: 'instruments', component: () => import('../views/policies/tabs/PolicyInstrumentsTab.vue') },
          {
            path: 'mandates',
            component: () => import('../views/policies/tabs/PolicyMandatesTab.vue'),
            meta: { permission: Permission.ViewMandate, trail: (p, t) => [...policyCrumbs(p, t), { label: 'Mandatos' }] },
          },
          {
            path: 'approvals',
            component: () => import('../views/policies/tabs/PolicyApprovalsTab.vue'),
            meta: { trail: (p, t) => [...policyCrumbs(p, t), { label: 'Aprovações' }] },
          },
          {
            path: 'confirmations',
            component: () => import('../views/policies/tabs/PolicyConfirmationsTab.vue'),
            meta: { permission: Permission.ViewOrder, trail: (p, t) => [...policyCrumbs(p, t), { label: 'Confirmations' }] },
          },
          { path: 'history', component: () => import('../views/policies/tabs/PolicyHistoryTab.vue') },
        ],
      },
      {
        path: 'policies/:policyId/mandates/new',
        component: () => import('../views/mandates/MandateNewView.vue'),
        props: true,
        meta: page(Permission.CreateMandate, (p, t) => [...policyCrumbs(p, t), { label: 'Novo mandato' }]),
      },
      {
        path: 'policies/:policyId/mandates/:mandateId',
        component: () => import('../views/mandates/MandateDetailView.vue'),
        props: true,
        meta: page(Permission.ViewMandate, mandateCrumbs),
      },
      {
        path: 'policies/:policyId/mandates/:mandateId/orders/new',
        component: () => import('../views/orders/OrderNewView.vue'),
        props: true,
        meta: page(Permission.CreateOrder, (p, t) => [...mandateCrumbs(p, t), { label: 'Nova boleta' }]),
      },
      {
        path: 'policies/:policyId/mandates/:mandateId/orders/:orderId',
        component: () => import('../views/orders/OrderDetailView.vue'),
        props: true,
        meta: page(Permission.ViewOrder, (p, t) => [...mandateCrumbs(p, t), { label: t.get(p.orderId) }]),
      },

      // Endereços antigos: mandato e boleta viram o caminho aninhado; listas soltas voltam para as políticas.
      { path: 'mandates/:mandateId', component: () => import('../views/EntityRedirectView.vue'), props: (r) => ({ kind: 'mandate', id: r.params.mandateId }) },
      { path: 'orders/:orderId', component: () => import('../views/EntityRedirectView.vue'), props: (r) => ({ kind: 'order', id: r.params.orderId }) },
      { path: 'mandates', redirect: paths.policies() },
      { path: 'orders', redirect: paths.policies() },
      { path: 'approvals', redirect: paths.policies() },

      // ---------- Usuários ----------
      {
        path: 'members',
        component: () => import('../views/organizations/MembersView.vue'),
        meta: page(Permission.ViewUsers, area('Usuários', 'Membros e grupos')),
      },
      {
        path: 'members/groups/:groupId',
        component: () => import('../views/organizations/GroupDetailView.vue'),
        props: true,
        meta: page(Permission.ViewUsers, (p, t) => [{ label: 'Usuários' }, { label: 'Membros e grupos', to: '/members' }, { label: t.get(p.groupId) }]),
      },
      { path: 'access', component: () => import('../views/organizations/AccessView.vue'), meta: page(undefined, area('Usuários', 'Cargos e Regras')) },

      // ---------- Organização ----------
      { path: 'setup', component: () => import('../views/setup/SetupView.vue'), meta: page(undefined, area('Organização', 'Setup da companhia')) },
      {
        path: 'counterparties',
        component: () => import('../views/setup/CounterpartiesView.vue'),
        meta: page(Permission.ViewCounterparties, area('Organização', 'Contrapartes')),
      },
      { path: 'timeline', component: () => import('../views/timeline/TimelineView.vue'), meta: page(undefined, area('Organização', 'Timeline')) },
    ],
  },
  {
    path: '/',
    component: () => import('../layouts/AuthLayout.vue'),
    children: [
      { path: 'login', component: () => import('../views/auth/LoginView.vue'), meta: { guest: true } },
      { path: 'register', component: () => import('../views/auth/RegisterView.vue'), meta: { guest: true } },
      { path: 'organizations', component: () => import('../views/organizations/OrganizationSelectView.vue') },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/' },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
  // Troca de aba dentro da mesma política não rola a página; telas novas começam do topo.
  scrollBehavior: (to, from, saved) => saved ?? (to.params.policyId && to.params.policyId === from.params.policyId && !to.params.mandateId ? false : { top: 0 }),
});

router.beforeEach(async (to) => {
  const session = useSessionStore();
  const organization = useOrganizationStore();

  if (to.meta.guest) {
    return session.isAuthenticated ? '/' : true;
  }

  if (!session.isAuthenticated) {
    return { path: '/login', query: to.fullPath !== '/' ? { redirect: to.fullPath } : {} };
  }

  if (!to.meta.organization) {
    return true;
  }

  if (!organization.hasOrganization) {
    return '/organizations';
  }

  // Primeira navegação após recarregar a página: busca setup e roles.
  if (!organization.current) {
    try {
      await organization.refresh();
    } catch {
      organization.clear();
      return '/organizations';
    }
  }

  // Sem a role da rota: volta para a visão geral (sempre acessível).
  if (to.meta.permission && !organization.can(to.meta.permission)) {
    return '/';
  }

  return true;
});
