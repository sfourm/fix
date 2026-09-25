<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { useSessionStore } from '@/application/stores/session.store';
import { Permission, type PermissionCode } from '@/domain/permissions';

const session = useSessionStore();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const route = useRoute();

interface NavItem {
  to: string;
  label: string;
  permission?: PermissionCode;
  count?: () => number;
  sub?: boolean;
}

/** Menu na ordem do processo do FIX: configurar → autorizar → operar → acompanhar. */
const sections: { title: string | null; items: NavItem[] }[] = [
  { title: null, items: [{ to: '/', label: 'Visão geral' }] },
  {
    title: '1 · Configurar',
    items: [
      { to: '/setup', label: 'Setup da companhia' },
      { to: '/counterparties', label: 'Contrapartes', permission: Permission.ViewCounterparties, sub: true },
      { to: '/members', label: 'Membros e grupos', permission: Permission.ViewUsers, sub: true },
      { to: '/access', label: 'Rules e alçadas', sub: true },
      { to: '/policies', label: 'Política de riscos', permission: Permission.ViewPolicy },
    ],
  },
  { title: '2 · Autorizar', items: [{ to: '/mandates', label: 'Mandatos', permission: Permission.ViewMandate }] },
  {
    title: '3 · Operar',
    items: [
      { to: '/orders', label: 'Boletas de hedge', permission: Permission.ViewOrder },
      { to: '/orders/open', label: 'Boletas em aberto', permission: Permission.ViewOrder, sub: true, count: () => queue.openConfirmations },
      { to: '/approvals', label: 'Fila de aprovação', count: () => queue.pendingApprovals },
    ],
  },
  { title: '4 · Acompanhar', items: [{ to: '/timeline', label: 'Timeline' }] },
];

const nav = computed(() =>
  sections
    .map((section) => ({ ...section, items: section.items.filter((i) => !i.permission || organization.can(i.permission)) }))
    .filter((section) => section.items.length > 0),
);

// '/orders' não deve ficar ativo em '/orders/open'; as demais rotas marcam o item pelo prefixo.
function isActive(item: NavItem): boolean {
  if (item.to === '/') return route.path === '/';
  if (item.to === '/orders') return route.path === '/orders' || /^\/orders\/(?!open$)/.test(route.path);
  return route.path === item.to || route.path.startsWith(`${item.to}/`);
}

// Navega antes de limpar o tenant: trocar a key do RouterView remontaria a view atual, que faria novas chamadas.
async function switchOrganization() {
  await router.push('/organizations');
  organization.clear();
  queue.reset();
}

async function logout() {
  session.logout();
  await router.push('/login');
  organization.reset();
  queue.reset();
}

onMounted(() => queue.refresh());
watch(() => organization.currentId, (id) => id && queue.refresh());
</script>

<template>
  <div class="shell">
    <aside class="sidebar">
      <div class="org">
        <span class="logo">{{ organization.current?.name.charAt(0).toUpperCase() ?? 'F' }}</span>
        <div class="org-info">
          <strong :title="organization.current?.name">{{ organization.current?.name ?? '…' }}</strong>
          <button class="btn-link btn small" type="button" @click="switchOrganization">Trocar organização</button>
        </div>
      </div>

      <nav class="nav">
        <template v-for="section in nav" :key="section.title ?? 'home'">
          <span v-if="section.title" class="nav-section">{{ section.title }}</span>
          <RouterLink
            v-for="item in section.items"
            :key="item.to"
            :to="item.to"
            class="nav-link"
            :class="{ active: isActive(item), sub: item.sub }"
          >
            <span>{{ item.label }}</span>
            <span v-if="item.count && item.count() > 0" class="badge badge-warning">{{ item.count() }}</span>
          </RouterLink>
        </template>
      </nav>

      <div class="user">
        <div class="user-info">
          <strong>{{ session.user?.fullName }}</strong>
          <span class="muted small">{{ session.user?.email }}</span>
        </div>
        <button class="btn btn-sm" type="button" @click="logout">Sair</button>
      </div>
    </aside>

    <main class="content">
      <RouterView :key="organization.currentId ?? ''" />
    </main>
  </div>
</template>

<style scoped>
.shell {
  display: grid;
  grid-template-columns: var(--sidebar-w) 1fr;
  min-height: 100vh;
}

.sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 20px 14px;
  background: var(--surface);
  border-right: 1px solid var(--border);
}

.org {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 6px;
}

.org-info {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  min-width: 0;
}

.org-info strong {
  max-width: 160px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.logo {
  display: grid;
  place-items: center;
  flex: none;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: var(--primary);
  color: #fff;
  font-weight: 700;
}

.nav {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  overflow-y: auto;
}

.nav-section {
  margin: 12px 10px 4px;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--text-muted);
}

.nav-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 7px 10px;
  border-radius: var(--radius-sm);
  color: var(--text);
  font-weight: 500;
}

.nav-link.sub {
  padding-left: 22px;
  font-size: 0.92rem;
}

.nav-link:hover {
  background: var(--surface-2);
  text-decoration: none;
}

.nav-link.active {
  background: var(--primary-soft);
  color: var(--primary);
}

.user {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 12px 6px 0;
  border-top: 1px solid var(--border);
}

.user-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.user-info span {
  overflow: hidden;
  text-overflow: ellipsis;
}

.content {
  padding: 28px 32px;
  min-width: 0;
}

@media (max-width: 800px) {
  .shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    position: static;
    height: auto;
  }

  .nav {
    flex-direction: row;
    flex-wrap: wrap;
  }

  .content {
    padding: 20px 16px;
  }
}
</style>
