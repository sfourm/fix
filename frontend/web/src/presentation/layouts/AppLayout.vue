<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { useSessionStore } from '@/application/stores/session.store';
import ThemeToggle from '../components/ThemeToggle.vue';
import { AREAS } from '../navigation';
import { crumbsFor, useTrailStore } from '../trail';

const session = useSessionStore();
const organization = useOrganizationStore();
const queue = useQueueStore();
const trail = useTrailStore();
const router = useRouter();
const route = useRoute();

/** Menu lateral como gaveta em telas pequenas. */
const menuOpen = ref(false);

/** Áreas e subitens visíveis para as roles do usuário. */
const areas = computed(() =>
  AREAS.filter((a) => !a.permission || organization.can(a.permission))
    .map((a) => ({ ...a, children: a.children.filter((c) => !c.permission || organization.can(c.permission)) }))
    .map((a) => ({ ...a, to: a.children[0]?.to ?? a.to }))
    .filter((a) => a.key === 'home' || a.key === 'policies' || a.children.length > 0),
);

const currentArea = computed(() => areas.value.find((a) => a.match.test(route.path)) ?? null);
const crumbs = computed(() => crumbsFor(route, trail));

/** Trilha aberta dentro de Políticas (política › mandato › boleta), exibida sob o item do menu. */
const policyTrail = computed(() => {
  const area = currentArea.value;
  if (area?.key !== 'policies') return [];
  // Passos que já são itens do menu (ex.: Exceções) não se repetem na trilha.
  return crumbs.value.slice(1).filter((c) => c.to && !area.children.some((child) => child.to === c.to));
});

/** "Voltar" sobe um nível no caminho (não depende do histórico do navegador). */
const parent = computed(() => [...crumbs.value].slice(0, -1).reverse().find((c) => c.to) ?? null);

const pending = computed(() => queue.pendingApprovals + queue.openConfirmations);

/** Remonta a tela ao trocar de entidade; abas da mesma política mantêm o cabeçalho. */
const viewKey = computed(() =>
  [organization.currentId, route.matched[1]?.path, route.params.policyId, route.params.mandateId, route.params.orderId, route.params.groupId].join('|'),
);

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

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') menuOpen.value = false;
}

onMounted(() => {
  queue.refresh();
  window.addEventListener('keydown', onKey);
});
onBeforeUnmount(() => window.removeEventListener('keydown', onKey));
watch(() => organization.currentId, (id) => id && queue.refresh());
watch(() => route.fullPath, () => (menuOpen.value = false));
</script>

<template>
  <div class="shell" :class="{ 'menu-open': menuOpen }">
    <div class="scrim" aria-hidden="true" @click="menuOpen = false" />

    <aside id="app-menu" class="sidebar">
      <div class="brand">
        <span class="logo" aria-hidden="true">FIX</span>
        <span class="brand-text">
          <strong>FIX</strong>
          <small>Gestão comercial e de risco</small>
        </span>
        <button class="close" type="button" aria-label="Fechar menu" @click="menuOpen = false">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6 6 18" /></svg>
        </button>
      </div>

      <button class="org" type="button" title="Trocar organização" @click="switchOrganization">
        <span class="org-mark">{{ organization.current?.name.charAt(0).toUpperCase() ?? 'F' }}</span>
        <span class="org-info">
          <strong :title="organization.current?.name">{{ organization.current?.name ?? '…' }}</strong>
          <span>Trocar organização</span>
        </span>
        <svg class="chev" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 9l4-4 4 4M8 15l4 4 4-4" /></svg>
      </button>

      <nav class="nav" aria-label="Menu principal">
        <template v-for="area in areas" :key="area.key">
          <RouterLink
            :to="area.to"
            class="nav-link"
            :class="{ active: currentArea?.key === area.key }"
            :aria-current="currentArea?.key === area.key ? 'page' : undefined"
          >
            <span class="nav-text">
              <span>{{ area.label }}</span>
              <small>{{ area.hint }}</small>
            </span>
            <span v-if="area.key === 'policies' && pending > 0" class="count" :title="`${queue.pendingApprovals} aguardando aprovação · ${queue.openConfirmations} confirmação(ões) em aberto`">{{ pending }}</span>
          </RouterLink>

          <div v-if="currentArea?.key === area.key && (area.children.length || policyTrail.length)" class="nav-sub">
            <RouterLink
              v-for="child in area.children"
              :key="child.to"
              :to="child.to"
              class="nav-child"
              :class="{ active: child.match.test(route.path) }"
            >
              {{ child.label }}
            </RouterLink>
            <RouterLink
              v-for="(step, i) in policyTrail"
              :key="step.to"
              :to="step.to!"
              class="nav-trail"
              :class="{ active: i === policyTrail.length - 1 }"
              :style="{ '--depth': i }"
            >
              {{ step.label }}
            </RouterLink>
          </div>
        </template>
      </nav>

      <div class="user">
        <span class="avatar">{{ session.user?.fullName?.charAt(0).toUpperCase() }}</span>
        <div class="user-info">
          <strong>{{ session.user?.fullName }}</strong>
          <span>{{ session.user?.email }}</span>
        </div>
        <button class="btn btn-sm" type="button" @click="logout">Sair</button>
      </div>
    </aside>

    <main class="main">
      <header class="topbar">
        <button class="icon-btn burger" type="button" aria-label="Abrir menu" aria-controls="app-menu" :aria-expanded="menuOpen" @click="menuOpen = true">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16M4 12h16M4 17h16" /></svg>
        </button>
        <button class="icon-btn" type="button" :disabled="!parent" :title="parent ? `Voltar para ${parent.label}` : ''" aria-label="Voltar" @click="parent && router.push(parent.to!)">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M15 5l-7 7 7 7" /></svg>
        </button>
        <nav class="crumbs" aria-label="Você está em">
          <template v-for="(c, i) in crumbs" :key="i">
            <span v-if="i > 0" class="sep">/</span>
            <RouterLink v-if="c.to && i < crumbs.length - 1" :to="c.to">{{ c.label }}</RouterLink>
            <span v-else :class="{ cur: i === crumbs.length - 1 }">{{ c.label }}</span>
          </template>
        </nav>
        <span v-if="organization.internalAccess" class="badge badge-info hide-sm" title="Você vê e edita esta organização para apoiá-la">Suporte FIX</span>
        <!-- Aguardando aprovação (decisão) e confirmações em aberto (a conferir, atrasado, divergente ou recusado) são coisas diferentes. -->
        <RouterLink v-if="queue.pendingApprovals > 0" to="/policies" class="badge badge-warning pending" :title="`${queue.pendingApprovals} mandato(s)/boleta(s) aguardando aprovação`">
          {{ queue.pendingApprovals }}<span class="hide-sm">&nbsp;aguardando aprovação</span>
        </RouterLink>
        <RouterLink
          v-if="queue.openConfirmations > 0"
          to="/policies"
          class="badge badge-info pending"
          :title="`${queue.openConfirmations} boleta(s) com confirmação em aberto: a conferir, atrasado, divergente ou recusado`"
        >
          {{ queue.openConfirmations }}<span class="hide-sm">&nbsp;confirmação(ões) em aberto</span>
        </RouterLink>
        <ThemeToggle compact />
      </header>

      <div class="wrap">
        <p v-if="organization.internalAccess" class="alert alert-info support-banner" role="note">
          <strong>Acesso de suporte FIX.</strong> Você vê e edita esta organização para apoiá-la; aprovações, confirmações e a alçada de emissão ficam com ela.
        </p>
        <RouterView v-slot="{ Component }">
          <div :key="viewKey" class="page-enter">
            <component :is="Component" />
          </div>
        </RouterView>
      </div>
    </main>
  </div>
</template>

<style scoped>
.shell {
  display: grid;
  grid-template-columns: var(--sidebar-w) minmax(0, 1fr);
  min-height: 100vh;
  min-height: 100dvh;
}

svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentcolor;
  stroke-width: 1.8;
  stroke-linecap: round;
  stroke-linejoin: round;
}

/* ---------- Menu lateral (vidro) ---------- */

/* Fixo na tela: o conteúdo rola e o menu fica parado ao lado. */
.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  z-index: 40;
  width: var(--sidebar-w);
  display: flex;
  flex-direction: column;
  gap: 14px;
  height: 100vh;
  height: 100dvh;
  padding: 18px 12px 14px;
  border-right: 1px solid var(--border);
  background: var(--glass);
  backdrop-filter: saturate(180%) blur(24px);
  -webkit-backdrop-filter: saturate(180%) blur(24px);
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 2px 8px 4px;
}

.logo {
  display: grid;
  place-items: center;
  flex: none;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: linear-gradient(145deg, #2c2c2e, #1d1d1f);
  box-shadow: inset 0 0 0 0.5px rgb(255 255 255 / 12%), 0 2px 8px rgb(0 0 0 / 18%);
  color: #e0ae4d;
  font-family: var(--font-display);
  font-size: 0.72rem;
  font-weight: 750;
  letter-spacing: 0.02em;
}

.brand-text {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}

.brand-text strong {
  font-family: var(--font-display);
  font-size: 1.05rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  line-height: 1.1;
}

.brand-text small {
  font-size: 0.72rem;
  color: var(--text-muted);
}

.close {
  display: none;
}

.org {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border: 0;
  border-radius: 14px;
  background: var(--fill);
  color: inherit;
  font: inherit;
  text-align: left;
  cursor: pointer;
  transition: background 0.2s var(--ease);
}

.org:hover {
  background: var(--fill-strong);
}

.org-mark {
  display: grid;
  place-items: center;
  flex: none;
  width: 30px;
  height: 30px;
  border-radius: 9px;
  background: var(--primary);
  color: var(--on-primary);
  font-family: var(--font-display);
  font-weight: 700;
}

.org-info {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}

.org-info strong {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 0.9rem;
  font-weight: 600;
}

.org-info span {
  font-size: 0.74rem;
  color: var(--text-muted);
}

.chev {
  flex: none;
  width: 15px;
  height: 15px;
  color: var(--text-muted);
}

.nav {
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 2px;
  overflow-y: auto;
  scrollbar-width: thin;
}

.nav-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 12px;
  color: var(--text-dim);
  transition:
    background 0.2s var(--ease),
    color 0.2s var(--ease);
}

.nav-text {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.nav-text span {
  font-size: 0.93rem;
  font-weight: 560;
}

.nav-text small {
  font-size: 0.72rem;
  color: var(--text-muted);
}

.nav-link:hover {
  background: var(--fill);
  color: var(--text);
}

.nav-link.active {
  background: var(--primary-soft);
  color: var(--text);
}

.nav-link.active .nav-text span {
  color: var(--primary);
  font-weight: 650;
}

.count {
  min-width: 22px;
  padding: 1px 7px;
  border-radius: 999px;
  background: var(--primary);
  color: var(--on-primary);
  font-size: 0.74rem;
  font-weight: 650;
  font-variant-numeric: tabular-nums;
  text-align: center;
}

.nav-sub {
  display: flex;
  flex-direction: column;
  gap: 1px;
  margin: 2px 0 6px 18px;
  padding-left: 10px;
  border-left: 1px solid var(--border-strong);
}

.nav-child {
  padding: 6px 10px;
  border-radius: 10px;
  color: var(--text-muted);
  font-size: 0.86rem;
  transition:
    background 0.2s var(--ease),
    color 0.2s var(--ease);
}

.nav-child:hover {
  background: var(--fill);
  color: var(--text);
}

.nav-child.active {
  color: var(--primary);
  font-weight: 600;
}

/* Trilha da cadeia política › mandato › boleta (recuo cresce a cada nível). */
.nav-trail {
  position: relative;
  padding: 4px 10px 4px calc(22px + var(--depth) * 12px);
  overflow: hidden;
  border-radius: 10px;
  color: var(--text-muted);
  font-size: 0.8rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.nav-trail::before {
  content: '';
  position: absolute;
  top: 50%;
  left: calc(10px + var(--depth) * 12px);
  width: 5px;
  height: 5px;
  margin-top: -2.5px;
  border-radius: 50%;
  background: var(--primary);
  opacity: 0.6;
}

.nav-trail:hover {
  background: var(--fill);
  color: var(--text);
}

.nav-trail.active {
  color: var(--primary);
  font-weight: 600;
}

.nav-trail.active::before {
  opacity: 1;
}

.user {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border-radius: 14px;
  background: var(--fill);
}

.avatar {
  display: grid;
  place-items: center;
  flex: none;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(145deg, var(--steel), var(--bronze));
  color: #fff; /* sobre o gradiente aço→bronze, nos dois temas */
  font-weight: 650;
  font-size: 0.85rem;
}

.user-info {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}

.user-info strong {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 0.86rem;
  font-weight: 600;
}

.user-info span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 0.72rem;
  color: var(--text-muted);
}

.user .btn {
  background: var(--surface-raised);
}

/* ---------- Barra superior (vidro) ---------- */

.main {
  grid-column: 2;
  min-width: 0;
}

.topbar {
  position: sticky;
  top: 0;
  z-index: 20;
  display: flex;
  align-items: center;
  gap: 10px;
  height: var(--topbar-h);
  padding: 0 28px;
  border-bottom: 1px solid var(--border);
  background: var(--glass);
  backdrop-filter: saturate(180%) blur(24px);
  -webkit-backdrop-filter: saturate(180%) blur(24px);
}

.icon-btn {
  display: grid;
  place-items: center;
  flex: none;
  width: 34px;
  height: 34px;
  border: 0;
  border-radius: 50%;
  background: var(--fill);
  color: var(--text);
  cursor: pointer;
  transition:
    background 0.2s var(--ease),
    transform 0.2s var(--ease);
}

.icon-btn svg {
  width: 17px;
  height: 17px;
}

.icon-btn:hover:not(:disabled) {
  background: var(--fill-strong);
}

.icon-btn:active:not(:disabled) {
  transform: scale(0.92);
}

.icon-btn:disabled {
  opacity: 0.3;
  cursor: default;
}

.burger {
  display: none;
}

.crumbs {
  display: flex;
  flex: 1;
  align-items: center;
  gap: 7px;
  min-width: 0;
  overflow: hidden;
  font-size: 0.86rem;
  white-space: nowrap;
}

.crumbs a {
  flex: none;
  color: var(--text-muted);
}

.crumbs a:hover {
  color: var(--primary);
}

.crumbs .sep {
  color: var(--text-faint);
}

.crumbs .cur {
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--text);
  font-weight: 600;
}

.pending:hover {
  color: var(--warning);
  filter: brightness(1.1);
}

.wrap {
  max-width: none;
  margin: 0 auto;
  padding: 28px 32px 56px;
}

.support-banner {
  margin: 0 0 16px;
}

.scrim {
  display: none;
}

/* ---------- Tablet / celular: menu vira gaveta ---------- */

@media (max-width: 960px) {
  .shell {
    grid-template-columns: 1fr;
  }

  .main {
    grid-column: 1;
  }

  .sidebar {
    position: fixed;
    inset: 0 auto 0 0;
    width: min(300px, 86vw);
    padding-top: max(18px, env(safe-area-inset-top));
    padding-bottom: max(14px, env(safe-area-inset-bottom));
    background: var(--surface);
    box-shadow: var(--shadow-lg);
    transform: translateX(-104%);
    transition: transform 0.38s var(--ease);
  }

  .menu-open .sidebar {
    transform: none;
  }

  .scrim {
    position: fixed;
    inset: 0;
    z-index: 30;
    display: block;
    background: rgb(0 0 0 / 32%);
    backdrop-filter: blur(3px);
    -webkit-backdrop-filter: blur(3px);
    opacity: 0;
    pointer-events: none;
    transition: opacity 0.3s var(--ease);
  }

  .menu-open .scrim {
    opacity: 1;
    pointer-events: auto;
  }

  .close {
    display: grid;
    place-items: center;
    width: 30px;
    height: 30px;
    border: 0;
    border-radius: 50%;
    background: var(--fill);
    color: var(--text-muted);
    cursor: pointer;
  }

  .close svg {
    width: 15px;
    height: 15px;
  }

  .burger {
    display: grid;
  }

  .topbar {
    padding: 0 16px;
    padding-top: env(safe-area-inset-top);
    height: calc(var(--topbar-h) + env(safe-area-inset-top));
  }

  .wrap {
    padding: 20px 16px 48px;
  }
}

@media (max-width: 560px) {
  .hide-sm {
    display: none;
  }

  .topbar {
    gap: 8px;
  }

  /* No celular a trilha mostra só onde se está. */
  .crumbs > :not(.cur) {
    display: none;
  }
}
</style>
