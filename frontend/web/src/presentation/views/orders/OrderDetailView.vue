<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import type { Counterparty } from '@/domain/counterparty';
import {
  approvalLabel,
  approvalTone,
  commodityLabel,
  confirmationLabel,
  confirmationTone,
  directionLabel,
  optionKindLabel,
  orderTypeLabel,
} from '@/domain/labels';
import { orderTypesFor, type Order, type OrderTermsDraft } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatDate, formatDateTime, formatNumber, formatUsd, orNull, today } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import DecisionModal from '../../components/DecisionModal.vue';
import OrderTermsForm from '../../components/order/OrderTermsForm.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import TimelineList from '../../components/TimelineList.vue';

const props = defineProps<{ orderId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const toast = useToast();
const { confirm } = useConfirm();

const { data: order, loading, error, status, load } = useLoader(() => api.orders.get(props.orderId));
const refreshKey = ref(0);

const pendingApproval = computed(() => order.value?.approval === 'PendingApproval');
const approved = computed(() => order.value?.approval === 'Approved');
const canManageConfirmation = computed(() => organization.can(Permission.ManageConfirmation));
const editable = computed(
  () => !!order.value && order.value.approval !== 'Rejected' && order.value.confirmation === 'Pending' && organization.can(Permission.UpdateOrder),
);

function applied(updated: Order, message: string) {
  order.value = updated;
  refreshKey.value++;
  queue.refresh();
  toast.success(message);
}

// ---------- Decisões (aprovação e confirmation) ----------
type Decision = 'approve' | 'reject' | 'divergence' | 'refusal';
const decision = ref<Decision | null>(null);
const decisions: Record<Decision, { title: string; label: string; required: boolean; danger?: boolean; message: string; noteLabel?: string }> = {
  approve: { title: 'Aprovar boleta', label: 'Aprovar', required: false, message: 'A aprovação consome o saldo do mandato.' },
  reject: { title: 'Rejeitar boleta', label: 'Rejeitar', required: true, danger: true, message: 'A boleta rejeitada não consome saldo.' },
  divergence: {
    title: 'Registrar divergência',
    label: 'Registrar',
    required: true,
    danger: true,
    message: 'O confirmation recebido não bate com a boleta.',
    noteLabel: 'O que diverge',
  },
  refusal: { title: 'Recusar confirmation', label: 'Recusar', required: true, danger: true, message: 'A contraparte não reconhece o trade.', noteLabel: 'Motivo' },
};

async function decide(note: string | null) {
  const kind = decision.value!;
  const actions: Record<Decision, () => Promise<Order>> = {
    approve: () => api.orders.approve(props.orderId, note),
    reject: () => api.orders.reject(props.orderId, note ?? ''),
    divergence: () => api.orders.markDivergent(props.orderId, note ?? ''),
    refusal: () => api.orders.refuse(props.orderId, note ?? ''),
  };
  const updated = await actions[kind]();
  decision.value = null;
  applied(updated, { approve: 'Boleta aprovada.', reject: 'Boleta rejeitada.', divergence: 'Divergência registrada.', refusal: 'Confirmation recusado.' }[kind]);
}

// ---------- Confirmation recebido ----------
const confirming = ref(false);
const receivedOn = ref(today());
const confirmSubmit = useSubmit();

async function confirmReceived() {
  const updated = await confirmSubmit.run(() => api.orders.confirm(props.orderId, receivedOn.value));
  if (updated) {
    confirming.value = false;
    applied(updated, 'Confirmation conferido.');
  }
}

async function resolve() {
  try {
    applied(await api.orders.resolve(props.orderId), 'Divergência sanada: confirmation conferido.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// ---------- Edição ----------
const editing = ref<{ counterpartyId: string; terms: OrderTermsDraft } | null>(null);
const counterparties = ref<Counterparty[]>([]);
const editSubmit = useSubmit();

async function openEdit() {
  const o = order.value!;
  editing.value = { counterpartyId: o.counterpartyId, terms: { ...o.terms } };
  editSubmit.reset();
  if (organization.can(Permission.ViewCounterparties)) {
    counterparties.value = await api.counterparties.list(true).catch(() => []);
  }
}

async function saveEdit() {
  const { counterpartyId, terms } = editing.value!;
  const updated = await editSubmit.run(() =>
    api.orders.update(props.orderId, {
      counterpartyId,
      terms: { ...terms, price: terms.price ?? 0, priceUnit: orNull(terms.priceUnit), notes: orNull(terms.notes) },
    }),
  );
  if (updated) {
    editing.value = null;
    applied(updated, 'Boleta atualizada.');
  }
}

async function remove() {
  const ok = await confirm({ title: 'Excluir boleta', message: 'Somente boletas não aprovadas podem ser excluídas.', confirmLabel: 'Excluir', danger: true });
  if (!ok) return;
  try {
    await api.orders.remove(props.orderId);
    queue.refresh();
    toast.success('Boleta excluída.');
    router.push('/orders');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(load);
</script>

<template>
  <StateBlock :loading="loading && !order" :error="status === 404 ? 'Boleta não encontrada.' : error" @retry="load">
    <template v-if="order">
      <PageHeader
        :title="`${directionLabel[order.terms.direction]} ${orderTypeLabel[order.terms.type]} ${order.terms.tenor}`"
        :subtitle="order.commodity ? `${commodityLabel[order.commodity]} · ${order.counterpartyName}` : `Moeda (US$) · ${order.counterpartyName}`"
      >
        <template #breadcrumb>
          <nav class="breadcrumb">
            <RouterLink to="/orders">Boletas de hedge</RouterLink><span>›</span>
            <RouterLink :to="`/mandates/${order.mandateId}`">{{ order.mandateTitle }}</RouterLink>
          </nav>
        </template>
        <template #actions>
          <template v-if="pendingApproval && organization.can(Permission.ApproveOrder)">
            <button class="btn btn-primary" @click="decision = 'approve'">Aprovar</button>
            <button class="btn btn-danger" @click="decision = 'reject'">Rejeitar</button>
          </template>
          <template v-if="approved && canManageConfirmation">
            <template v-if="order.confirmation === 'Pending'">
              <button class="btn btn-primary" @click="confirming = true; confirmSubmit.reset()">Confirmation recebido</button>
              <button class="btn btn-danger" @click="decision = 'divergence'">Divergência</button>
              <button class="btn btn-danger" @click="decision = 'refusal'">Recusa</button>
            </template>
            <button v-if="order.confirmation === 'Divergent'" class="btn btn-primary" @click="resolve">Sanar divergência</button>
          </template>
          <button v-if="editable" class="btn" @click="openEdit">Editar</button>
          <button v-if="!approved && organization.can(Permission.DeleteOrder)" class="btn btn-danger" @click="remove">Excluir</button>
        </template>
      </PageHeader>

      <div class="grid-2">
        <section class="card">
          <header class="card-header">
            <h2>Boleta</h2>
            <StatusBadge :label="approvalLabel[order.approval]" :tone="approvalTone[order.approval]" />
          </header>
          <dl class="details card-body">
            <dt>Data do trade</dt>
            <dd>{{ formatDate(order.terms.tradeDate) }}</dd>
            <dt>Volume</dt>
            <dd>{{ order.terms.type === 'Ndf' ? formatUsd(order.terms.notionalUsd) : `${formatNumber(order.terms.lots)} lotes` }}</dd>
            <dt>{{ order.terms.type === 'Option' ? 'Strike' : order.terms.type === 'Ndf' ? 'Taxa' : 'Preço' }}</dt>
            <dd>{{ formatNumber(order.terms.price, 4) }} {{ order.terms.priceUnit ?? '' }}</dd>
            <template v-if="order.terms.type === 'Option'">
              <dt>Opção</dt>
              <dd>{{ order.terms.optionKind ? optionKindLabel[order.terms.optionKind] : '—' }} · prêmio {{ formatNumber(order.terms.premium, 4) }}</dd>
            </template>
            <dt>Observações</dt>
            <dd>{{ order.terms.notes ?? '—' }}</dd>
            <dt>Decisão</dt>
            <dd>{{ order.decidedAt ? `${formatDateTime(order.decidedAt)}${order.decisionNote ? ` · ${order.decisionNote}` : ''}` : '—' }}</dd>
          </dl>
        </section>

        <section class="card">
          <header class="card-header">
            <h2>Confirmation</h2>
            <StatusBadge v-if="approved" :label="confirmationLabel[order.confirmation]" :tone="confirmationTone[order.confirmation]" />
          </header>
          <div class="card-body stack">
            <p v-if="!approved" class="muted" style="margin: 0">O confirmation só se aplica depois da aprovação da boleta.</p>
            <template v-else>
              <p v-if="order.confirmationOverdue" class="alert alert-error">Confirmation pendente há mais de 2 dias úteis.</p>
              <dl class="details">
                <dt>Recebido em</dt>
                <dd>{{ formatDate(order.confirmedOn) }}</dd>
                <dt>Observação</dt>
                <dd>{{ order.confirmationNote ?? '—' }}</dd>
              </dl>
            </template>
          </div>
        </section>

        <section class="card" style="grid-column: 1 / -1">
          <header class="card-header"><h2>Histórico</h2></header>
          <TimelineList entity-type="Order" :entity-id="order.id" :refresh-key="refreshKey" />
        </section>
      </div>
    </template>
  </StateBlock>

  <DecisionModal
    v-if="decision"
    :title="decisions[decision].title"
    :message="decisions[decision].message"
    :confirm-label="decisions[decision].label"
    :note-required="decisions[decision].required"
    :note-label="decisions[decision].noteLabel"
    :danger="decisions[decision].danger"
    :action="decide"
    @close="decision = null"
  />

  <BaseModal v-if="confirming" title="Confirmation recebido" width="420px" @close="confirming = false">
    <form id="confirm-form" class="stack" @submit.prevent="confirmReceived">
      <p v-if="confirmSubmit.error.value" class="alert alert-error">{{ confirmSubmit.error.value }}</p>
      <div class="field">
        <label for="received-on">Data de recebimento</label>
        <input id="received-on" v-model="receivedOn" class="input" type="date" required />
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="confirming = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="confirm-form" :disabled="confirmSubmit.submitting.value">Conferido</button>
    </template>
  </BaseModal>

  <BaseModal v-if="editing && order" title="Editar boleta" width="760px" @close="editing = null">
    <form id="order-edit-form" class="stack" @submit.prevent="saveEdit">
      <p v-if="editSubmit.error.value" class="alert alert-error">{{ editSubmit.error.value }}</p>
      <div class="field">
        <label for="edit-counterparty">Contraparte</label>
        <select id="edit-counterparty" v-model="editing.counterpartyId" class="input">
          <option :value="order.counterpartyId">{{ order.counterpartyName }}</option>
          <option v-for="c in counterparties.filter((x) => x.id !== order?.counterpartyId)" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
      </div>
      <OrderTermsForm v-model="editing.terms" :types="orderTypesFor(order.terms.type === 'Ndf' ? 'Currency' : 'Pricing')" :field-error="editSubmit.fieldError" />
    </form>
    <template #footer>
      <button class="btn" type="button" @click="editing = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="order-edit-form" :disabled="editSubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>
</template>
