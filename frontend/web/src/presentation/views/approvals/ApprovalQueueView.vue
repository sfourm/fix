<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { complianceLabel, complianceTone, directionLabel, mandateTypeLabel, orderTypeLabel } from '@/domain/labels';
import type { Mandate } from '@/domain/mandate';
import type { Order } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { useLoader } from '../../composables/useAsync';
import { formatDate, formatNumber, formatUsd } from '../../composables/format';
import DecisionModal from '../../components/DecisionModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();

const { data, loading, error, load } = useLoader(async () => {
  const [mandates, orders] = await Promise.all([
    organization.can(Permission.ViewMandate) ? api.mandates.list({ status: 'PendingApproval', pageSize: 100 }).then((p) => p.items) : [],
    organization.can(Permission.ViewOrder) ? api.orders.list({ approval: 'PendingApproval', pageSize: 100 }).then((p) => p.items) : [],
  ]);
  return { mandates, orders };
});

const canApproveMandate = computed(() => organization.can(Permission.ApproveMandate));
const canApproveException = computed(() => organization.can(Permission.ApproveException));
const canApproveOrder = computed(() => organization.can(Permission.ApproveOrder));

/** Mandato fora da política só pode ser aprovado por quem tem approve_exception. */
const canApprove = (m: Mandate) => canApproveMandate.value && (m.compliance.status === 'Within' || canApproveException.value);

type Pending = { kind: 'mandate'; item: Mandate; approve: boolean } | { kind: 'order'; item: Order; approve: boolean };
const deciding = ref<Pending | null>(null);

const modal = computed(() => {
  const d = deciding.value;
  if (!d) return null;
  const what = d.kind === 'mandate' ? 'mandato' : 'boleta';
  return d.approve
    ? { title: `Aprovar ${what}`, label: 'Aprovar', required: false, danger: false }
    : { title: `Rejeitar ${what}`, label: 'Rejeitar', required: true, danger: true };
});

async function decide(note: string | null) {
  const d = deciding.value!;
  if (d.kind === 'mandate') {
    await (d.approve ? api.mandates.approve(d.item.id, note) : api.mandates.reject(d.item.id, note ?? ''));
  } else {
    await (d.approve ? api.orders.approve(d.item.id, note) : api.orders.reject(d.item.id, note ?? ''));
  }
  deciding.value = null;
  await Promise.all([load(), queue.refresh()]);
}

onMounted(() => {
  load();
  queue.refresh();
});
</script>

<template>
  <PageHeader
    title="Fila de aprovação"
    subtitle="Mandatos e boletas emitidos sem alçada. Pendentes não consomem saldo; mandatos fora da política exigem alçada de exceção."
  />

  <StateBlock :loading="loading && !data" :error="error" @retry="load">
    <div v-if="data" class="stack">
      <section class="card">
        <header class="card-header">
          <h2>Mandatos</h2>
          <span class="badge badge-warning">{{ data.mandates.length }}</span>
        </header>
        <div v-if="data.mandates.length" class="table-wrap">
          <table class="table">
            <thead>
              <tr><th>Mandato</th><th>Tipo</th><th>Eixo</th><th>Quantidade</th><th>Enquadramento</th><th /></tr>
            </thead>
            <tbody>
              <tr v-for="m in data.mandates" :key="m.id">
                <td><RouterLink :to="`/mandates/${m.id}`"><strong>{{ m.terms.title }}</strong></RouterLink></td>
                <td>{{ mandateTypeLabel[m.type] }}</td>
                <td class="muted small">{{ m.policyCode.toUpperCase() }} · {{ m.axisCode }}</td>
                <td>{{ formatNumber(m.terms.quantity) }}</td>
                <td>
                  <StatusBadge :label="complianceLabel[m.compliance.status]" :tone="complianceTone[m.compliance.status]" />
                  <div class="muted small">{{ m.compliance.reason }}</div>
                </td>
                <td class="actions">
                  <template v-if="canApproveMandate">
                    <button class="btn btn-sm btn-primary" :disabled="!canApprove(m)" :title="canApprove(m) ? '' : 'Exige a role approve_exception'" @click="deciding = { kind: 'mandate', item: m, approve: true }">
                      Aprovar
                    </button>
                    <button class="btn btn-sm btn-danger" @click="deciding = { kind: 'mandate', item: m, approve: false }">Rejeitar</button>
                  </template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-else class="card-body muted" style="margin: 0">Nenhum mandato aguardando aprovação.</p>
      </section>

      <section class="card">
        <header class="card-header">
          <h2>Boletas</h2>
          <span class="badge badge-warning">{{ data.orders.length }}</span>
        </header>
        <div v-if="data.orders.length" class="table-wrap">
          <table class="table">
            <thead>
              <tr><th>Boleta</th><th>Mandato</th><th>Volume</th><th>Contraparte</th><th>Trade</th><th /></tr>
            </thead>
            <tbody>
              <tr v-for="o in data.orders" :key="o.id">
                <td>
                  <RouterLink :to="`/orders/${o.id}`"><strong>{{ directionLabel[o.terms.direction] }} {{ orderTypeLabel[o.terms.type] }} {{ o.terms.tenor }}</strong></RouterLink>
                </td>
                <td class="small">{{ o.mandateTitle }}</td>
                <td>{{ o.terms.type === 'Ndf' ? formatUsd(o.terms.notionalUsd) : `${formatNumber(o.terms.lots)} lotes` }}</td>
                <td>{{ o.counterpartyName }}</td>
                <td>{{ formatDate(o.terms.tradeDate) }}</td>
                <td class="actions">
                  <template v-if="canApproveOrder">
                    <button class="btn btn-sm btn-primary" @click="deciding = { kind: 'order', item: o, approve: true }">Aprovar</button>
                    <button class="btn btn-sm btn-danger" @click="deciding = { kind: 'order', item: o, approve: false }">Rejeitar</button>
                  </template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-else class="card-body muted" style="margin: 0">Nenhuma boleta aguardando aprovação.</p>
      </section>
    </div>
  </StateBlock>

  <DecisionModal
    v-if="deciding && modal"
    :title="modal.title"
    :confirm-label="modal.label"
    :note-required="modal.required"
    :danger="modal.danger"
    :action="decide"
    @close="deciding = null"
  />
</template>
