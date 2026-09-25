<script setup lang="ts">
import { computed, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { complianceLabel, complianceTone, directionLabel, mandateTypeLabel, orderTypeLabel } from '@/domain/labels';
import type { Mandate } from '@/domain/mandate';
import type { Order } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { formatDate, formatNumber, formatUsd } from '../../../composables/format';
import DecisionModal from '../../../components/DecisionModal.vue';
import StatusBadge from '../../../components/StatusBadge.vue';
import { paths } from '../../../paths';
import { usePolicyContext } from '../policy-context';

/** Mandatos e boletas desta política emitidos sem alçada: aguardam decisão de quem está acima no organograma. */
const { policy, work, reloadWork } = usePolicyContext();
const api = useApi();
const organization = useOrganizationStore();

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
  await reloadWork();
}
</script>

<template>
  <div v-if="!work" class="card card-body muted">Carregando pendências…</div>
  <div v-else class="stack">
    <p class="lead small">Pendentes não consomem saldo. Decide quem tem a role e está acima de quem emitiu no organograma; mandatos fora da política exigem alçada de exceção.</p>

    <section class="card">
      <header class="card-header">
        <h2>Mandatos</h2>
        <span class="badge" :class="work.pendingMandates.length ? 'badge-warning' : ''">{{ work.pendingMandates.length }}</span>
      </header>
      <div v-if="work.pendingMandates.length" class="table-wrap">
        <table v-columns="'approvals-mandates'" class="table">
          <thead>
            <tr><th>Mandato</th><th>Tipo</th><th>Eixo</th><th>Quantidade</th><th>Enquadramento</th><th /></tr>
          </thead>
          <tbody>
            <tr v-for="m in work.pendingMandates" :key="m.id">
              <td><RouterLink :to="paths.mandate(policy.id, m.id)"><strong>{{ m.terms.title }}</strong></RouterLink></td>
              <td>{{ mandateTypeLabel[m.type] }}</td>
              <td class="muted small">{{ m.axisCode }} · {{ m.axisTitle }}</td>
              <td class="num">{{ formatNumber(m.terms.quantity) }}</td>
              <td>
                <StatusBadge :label="complianceLabel[m.compliance.status]" :tone="complianceTone[m.compliance.status]" />
                <span class="sub">{{ m.compliance.reason }}</span>
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
        <span class="badge" :class="work.pendingOrders.length ? 'badge-warning' : ''">{{ work.pendingOrders.length }}</span>
      </header>
      <div v-if="work.pendingOrders.length" class="table-wrap">
        <table v-columns="'approvals-orders'" class="table">
          <thead>
            <tr><th>Boleta</th><th>Mandato</th><th>Volume</th><th>Contraparte</th><th>Trade</th><th /></tr>
          </thead>
          <tbody>
            <tr v-for="o in work.pendingOrders" :key="o.id">
              <td>
                <RouterLink :to="paths.order(policy.id, o.mandateId, o.id)"><strong>{{ directionLabel[o.terms.direction] }} {{ orderTypeLabel[o.terms.type] }} {{ o.terms.tenor }}</strong></RouterLink>
              </td>
              <td class="small"><RouterLink :to="paths.mandate(policy.id, o.mandateId)">{{ o.mandateTitle }}</RouterLink></td>
              <td class="num">{{ o.terms.type === 'Ndf' ? formatUsd(o.terms.notionalUsd) : `${formatNumber(o.terms.lots)} lotes` }}</td>
              <td>{{ o.counterpartyName }}</td>
              <td class="num">{{ formatDate(o.terms.tradeDate) }}</td>
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
