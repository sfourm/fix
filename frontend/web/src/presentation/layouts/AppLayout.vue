<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { useSessionStore } from '@/application/stores/session.store';
import { AREAS, isTabActive, type AreaTab } from '../navigation';

const session = useSessionStore();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const route = useRoute();

/** Áreas e abas visíveis para as roles do usuário (área sem nenhuma aba visível some do menu). */
const areas = computed(() =>
  AREAS.map((area) => ({ ...area, tabs: area.tabs.filter((t) => !t.permission || organization.can(t.permission)) })).filter(
    (area) => area.tabs.length > 0,
  ),
);

const currentArea = computed(() => areas.value.find((area) => area.tabs.some((tab) => isTabActive(tab, route.path))) ?? null);

const count = (tab: AreaTab) =>
  tab.counter === 'approvals' ? queue.pendingApprovals : tab.counter === 'confirmations' ? queue.openConfirmations : 0;

/** Pendências de uma área inteira, exibidas no item do menu lateral. */
const areaCount = (tabs: AreaTab[]) => tabs.reduce((sum, tab) => sum + count(tab), 0);

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

      <nav class="nav" aria-label="Menu principal">
        <RouterLink
          v-for="area in areas"
          :key="area.key"
          :to="area.tabs[0]!.to"
          class="nav-link"
          :class="{ active: currentArea?.key === area.key }"
          :aria-current="currentArea?.key === area.key ? 'page' : undefined"
        >
          <span>{{ area.label }}</span>
          <span v-if="areaCount(area.tabs) > 0" class="badge badge-warning">{{ areaCount(area.tabs) }}</span>
        </RouterLink>
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
      <p v-if="organization.internalAccess" class="alert alert-info support-banner" role="note">
        <strong>Acesso de suporte FIX.</strong> Você vê e edita esta organização para apoiá-la; aprovações, confirmations e a alçada de emissão ficam com ela.
      </p>
      <nav v-if="currentArea && currentArea.tabs.length > 1" class="tabs area-tabs" :aria-label="currentArea.label">
        <RouterLink
          v-for="tab in currentArea.tabs"
          :key="tab.to"
          :to="tab.to"
          class="tab"
          :class="{ active: isTabActive(tab, route.path) }"
        >
          {{ tab.label }}
          <span v-if="count(tab) > 0" class="badge badge-warning">{{ count(tab) }}</span>
        </RouterLink>
      </nav>
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


.nav-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px 12px;
  border-radius: var(--radius-sm);
  color: var(--text);
  font-weight: 500;
}


.nav-link:hover {
  background: var(--surface-2);
  text-decoration: none;
}

.nav-link.active {
  background: var(--primary-soft);
  color: var(--primary);
}

.support-banner {
  margin: -8px 0 16px;
}

.area-tabs {
  margin: -8px 0 20px;
}

.area-tabs .tab {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  text-decoration: none;
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
