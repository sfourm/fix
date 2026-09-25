import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useSessionStore } from '@/application/stores/session.store';
import { Permission, type PermissionCode } from '@/domain/permissions';

declare module 'vue-router' {
  interface RouteMeta {
    /** Rota pública (login/cadastro); usuário logado é redirecionado. */
    guest?: boolean;
    /** Exige organização selecionada (tenant). */
    organization?: boolean;
    permission?: PermissionCode;
  }
}

const page = (permission?: PermissionCode) => ({ organization: true, permission });

const routes: RouteRecordRaw[] = [
  // AppLayout primeiro: com dois pais em '/', o primeiro definido vence para a rota raiz.
  {
    path: '/',
    component: () => import('../layouts/AppLayout.vue'),
    meta: { organization: true },
    children: [
      { path: '', component: () => import('../views/home/HomeView.vue'), meta: page() },

      // 1 · Configurar
      { path: 'setup', component: () => import('../views/setup/SetupView.vue'), meta: page() },
      {
        path: 'counterparties',
        component: () => import('../views/setup/CounterpartiesView.vue'),
        meta: page(Permission.ViewCounterparties),
      },
      { path: 'members', component: () => import('../views/organizations/MembersView.vue'), meta: page(Permission.ViewUsers) },
      { path: 'access', component: () => import('../views/organizations/AccessView.vue'), meta: page() },
      { path: 'policies', component: () => import('../views/policies/PolicyListView.vue'), meta: page(Permission.ViewPolicy) },
      {
        path: 'policies/:policyId',
        component: () => import('../views/policies/PolicyDetailView.vue'),
        props: true,
        meta: page(Permission.ViewPolicy),
      },

      // 2 · Autorizar
      { path: 'mandates', component: () => import('../views/mandates/MandateListView.vue'), meta: page(Permission.ViewMandate) },
      { path: 'mandates/new', component: () => import('../views/mandates/MandateNewView.vue'), meta: page(Permission.CreateMandate) },
      {
        path: 'mandates/:mandateId',
        component: () => import('../views/mandates/MandateDetailView.vue'),
        props: true,
        meta: page(Permission.ViewMandate),
      },

      // 3 · Operar
      { path: 'orders', component: () => import('../views/orders/OrderListView.vue'), meta: page(Permission.ViewOrder) },
      { path: 'orders/new', component: () => import('../views/orders/OrderNewView.vue'), meta: page(Permission.CreateOrder) },
      { path: 'orders/open', component: () => import('../views/orders/OpenOrdersView.vue'), meta: page(Permission.ViewOrder) },
      {
        path: 'orders/:orderId',
        component: () => import('../views/orders/OrderDetailView.vue'),
        props: true,
        meta: page(Permission.ViewOrder),
      },
      { path: 'approvals', component: () => import('../views/approvals/ApprovalQueueView.vue'), meta: page() },

      // 4 · Acompanhar
      { path: 'timeline', component: () => import('../views/timeline/TimelineView.vue'), meta: page() },
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
