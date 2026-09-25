<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { commodityLabel, policyStatusLabel, policyStatusTone, sectorLabel } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import { useLoader } from '../../composables/useAsync';
import { formatDate, formatNumber } from '../../composables/format';
import PageHeader from '../../components/PageHeader.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import TimelineList from '../../components/TimelineList.vue';

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const setup = computed(() => organization.current);

const policies = useLoader(() =>
  organization.can(Permission.ViewPolicy) ? api.policies.list({ page: 1, pageSize: 50 }) : Promise.resolve(null),
);
const mandates = useLoader(() =>
  organization.can(Permission.ViewMandate) ? api.mandates.list({ status: 'Active', pageSize: 100 }) : Promise.resolve(null),
);

const members = useLoader(() => (organization.can(Permission.ViewUsers) ? api.organizations.members() : Promise.resolve(null)));

const activePolicy = computed(() => policies.data.value?.items.find((p) => p.status === 'Active') ?? null);
const draftPolicy = computed(() => policies.data.value?.items.find((p) => p.status === 'Draft' || p.status === 'UnderApproval') ?? null);
const outsideMandates = computed(() => mandates.data.value?.items.filter((m) => m.compliance.status === 'Outside').length ?? 0);

/** Passos do processo que ainda faltam configurar (guia do FIX: configurar → autorizar → operar). */
const checklist = computed(() => [
  { done: !!setup.value?.profile.activeCrop, label: 'Identificação e safra ativa', to: '/setup' },
  { done: (setup.value?.commodities.length ?? 0) > 0, label: 'Commodities e capacidade', to: '/setup' },
  { done: setup.value?.budget.economicFloor != null, label: 'Orçamento e gatilhos (piso econômico)', to: '/setup' },
  { done: !!activePolicy.value, label: 'Política de riscos vigente (aprovada em ata)', to: '/policies' },
  { done: (mandates.data.value?.totalCount ?? 0) > 0, label: 'Mandato ativo para operar', to: '/mandates' },
]);

onMounted(() => {
  policies.load();
  mandates.load();
  members.load();
  queue.refresh();
});
</script>

<template>
  <PageHeader
    title="Visão geral"
    :subtitle="setup ? `${setup.profile.corporateName} · ${sectorLabel[setup.profile.sector]} · safra ${setup.profile.activeCrop ?? '—'}` : null"
  />

  <div class="kpis">
    <RouterLink v-if="organization.can(Permission.ViewPolicy)" to="/policies" class="kpi card">
      <span class="muted small">Política de riscos</span>
      <template v-if="activePolicy">
        <strong>{{ activePolicy.code }} · {{ activePolicy.version }}</strong>
        <span class="small">vigente até {{ formatDate(activePolicy.validTo) }}</span>
      </template>
      <template v-else>
        <strong>Sem política vigente</strong>
        <StatusBadge v-if="draftPolicy" :label="policyStatusLabel[draftPolicy.status]" :tone="policyStatusTone[draftPolicy.status]" />
      </template>
    </RouterLink>
    <RouterLink v-if="organization.can(Permission.ViewMandate)" to="/mandates" class="kpi card">
      <span class="muted small">Mandatos ativos</span>
      <strong>{{ mandates.data.value?.totalCount ?? '…' }}</strong>
      <span class="small" :class="{ danger: outsideMandates > 0 }">{{ outsideMandates }} fora da política (exceção aprovada)</span>
    </RouterLink>
    <RouterLink to="/approvals" class="kpi card">
      <span class="muted small">Fila de aprovação</span>
      <strong>{{ queue.pendingApprovals }}</strong>
      <span class="small">mandatos e boletas aguardando alçada</span>
    </RouterLink>
    <RouterLink v-if="organization.can(Permission.ViewOrder)" to="/orders/open" class="kpi card">
      <span class="muted small">Confirmations em aberto</span>
      <strong>{{ queue.openConfirmations }}</strong>
      <span class="small">pendentes, divergentes ou recusados</span>
    </RouterLink>
  </div>

  <div class="grid-2">
    <section class="card">
      <header class="card-header"><h2>Como começar</h2></header>
      <ol class="checklist card-body">
        <li v-for="step in checklist" :key="step.label" :class="{ done: step.done }">
          <span aria-hidden="true">{{ step.done ? '✓' : '○' }}</span>
          <RouterLink :to="step.to">{{ step.label }}</RouterLink>
        </li>
      </ol>
    </section>

    <section class="card">
      <header class="card-header"><h2>Orçamento e gatilhos</h2><RouterLink to="/setup" class="small">editar</RouterLink></header>
      <dl class="details card-body">
        <dt>Custo caixa (gatilho)</dt>
        <dd>{{ formatNumber(setup?.budget.cashCost) }} c/lb</dd>
        <dt>Piso econômico</dt>
        <dd>{{ formatNumber(setup?.budget.economicFloor) }} c/lb</dd>
        <dt>Preço equivalente</dt>
        <dd>{{ formatNumber(setup?.budget.equivalentPrice) }} c/lb</dd>
        <dt>Commodities</dt>
        <dd>{{ setup?.commodities.map((c) => commodityLabel[c.commodity]).join(' · ') || '—' }}</dd>
      </dl>
    </section>

    <section class="card" style="grid-column: 1 / -1">
      <header class="card-header"><h2>Últimas alterações</h2><RouterLink to="/timeline" class="small">ver timeline</RouterLink></header>
      <TimelineList :limit="8" :members="members.data.value" compact />
    </section>
  </div>
</template>

<style scoped>
.kpis {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

.kpi {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 16px 18px;
  color: var(--text);
}

.kpi:hover {
  text-decoration: none;
  border-color: var(--primary);
}

.kpi strong {
  font-size: 1.35rem;
}

.danger {
  color: var(--danger);
}

.checklist {
  list-style: none;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.checklist li {
  display: flex;
  gap: 10px;
}

.checklist li.done {
  color: var(--text-muted);
}

.checklist li.done a {
  color: var(--success);
}
</style>
