<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { useSessionStore } from '@/application/stores/session.store';
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
import type { Mandate } from '@/domain/mandate';
import { orderStamps, orderTypesFor, type Order, type OrderTermsDraft } from '@/domain/order';
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
import AuditPanel from '../../components/audit/AuditPanel.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';

/**
 * Boleta dentro de política › mandato (cadeia 1:N) ou, sem mandato, na página de exceções. Enquadramento e carimbos de
 * desvio sempre à vista; sem mandato, dá para vincular um depois (a posteriori, carimbo permanente).
 */
const props = defineProps<{ policyId?: string; mandateId?: string; orderId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const toast = useToast();
const { confirm } = useConfirm();
const trail = useTrailStore();
const route = useRoute();

// ---------- Abas (mesmo formato da política e do mandato; a aba fica na URL) ----------
type Tab = 'overview' | 'confirmation' | 'audit';
const tab = computed<Tab>(() => (['confirmation', 'audit'].includes(route.query.tab as string) ? (route.query.tab as Tab) : 'overview'));
const selectTab = (key: Tab) => router.replace({ query: key === 'overview' ? {} : { tab: key } });
const session = useSessionStore();

const { data: order, loading, error, status, load } = useLoader(async () => {
  const o = await api.orders.get(props.orderId);
  trail.set(o.id, `${o.code} · ${directionLabel[o.terms.direction]} ${orderTypeLabel[o.terms.type]} ${o.terms.tenor}`);
  if (!o.mandateId) {
    // Sem mandato: o lugar dela é a página de exceções.
    if (props.mandateId) router.replace(paths.orderWithoutMandate(o.id));
    return o;
  }
  // Aberta pelo mandato errado (link antigo/copiado ou recém-vinculada): vai para o caminho certo.
  const m = await api.mandates.get(o.mandateId);
  if (o.mandateId !== props.mandateId || m.policyId !== props.policyId) router.replace(paths.order(m.policyId, m.id, o.id));
  trail.set(m.id, `${m.code} · ${m.terms.title}`);
  trail.set(m.policyId, `${m.policyCode.toUpperCase()} ${m.policyVersion}`);
  return o;
});
const refreshKey = ref(0);

const pendingApproval = computed(() => order.value?.approval === 'PendingApproval');
const approved = computed(() => order.value?.approval === 'Approved');
const canManageConfirmation = computed(() => organization.can(Permission.ManageConfirmation));
/** Segregação (FIX2 · I-06): quem executou a boleta não confere a confirmação dela. */
const isExecutor = computed(() => !!order.value && order.value.requestedBy === session.user?.id);
const stamps = computed(() => (order.value ? orderStamps(order.value) : []));
/** Confirmação precisa de atenção: atrasada, divergente ou recusada. */
const confirmationAttention = computed(
  () => !!order.value && order.value.approval === 'Approved' && (order.value.confirmationOverdue || ['Divergent', 'Refused'].includes(order.value.confirmation)),
);
const tabs = computed(() => [
  { key: 'overview' as const, label: 'Visão geral' },
  { key: 'confirmation' as const, label: 'Confirmação', alert: confirmationAttention.value },
  { key: 'audit' as const, label: 'Auditoria' },
]);
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
  approve: { title: 'Aprovar boleta', label: 'Aprovar', required: false, message: 'A aprovação consome o saldo do mandato. Boleta FORA continua FORA depois de aprovada — a responsabilidade é de quem aprova.' },
  reject: { title: 'Rejeitar boleta', label: 'Rejeitar', required: true, danger: true, message: 'A boleta rejeitada não consome saldo.' },
  divergence: {
    title: 'Registrar divergência',
    label: 'Registrar',
    required: true,
    danger: true,
    message: 'A confirmação recebido não bate com a boleta.',
    noteLabel: 'O que diverge',
  },
  refusal: { title: 'Recusar confirmação', label: 'Recusar', required: true, danger: true, message: 'A contraparte não reconhece a operação.', noteLabel: 'Motivo' },
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
  applied(updated, { approve: 'Boleta aprovada.', reject: 'Boleta rejeitada.', divergence: 'Divergência registrada.', refusal: 'Confirmação recusada.' }[kind]);
}

// ---------- Confirmação recebida ----------
const confirming = ref(false);
const receivedOn = ref(today());
const confirmSubmit = useSubmit();

async function confirmReceived() {
  const updated = await confirmSubmit.run(() => api.orders.confirm(props.orderId, receivedOn.value));
  if (updated) {
    confirming.value = false;
    applied(updated, 'Confirmação conferida.');
  }
}

async function resolve() {
  try {
    applied(await api.orders.resolve(props.orderId), 'Confirmação sanada e conferida.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// ---------- Edição ----------
const editing = ref<{ counterpartyId: string; terms: OrderTermsDraft; justification: string } | null>(null);
const counterparties = ref<Counterparty[]>([]);
const editSubmit = useSubmit();

async function openEdit() {
  const o = order.value!;
  editing.value = { counterpartyId: o.counterpartyId, terms: { ...o.terms }, justification: o.deviationNote ?? '' };
  editSubmit.reset();
  if (organization.can(Permission.ViewCounterparties)) {
    counterparties.value = await api.counterparties.list(true).catch(() => []);
  }
}

async function saveEdit() {
  const { counterpartyId, terms, justification } = editing.value!;
  const updated = await editSubmit.run(() =>
    api.orders.update(props.orderId, {
      counterpartyId,
      terms: {
        ...terms,
        price: terms.price ?? 0,
        priceUnit: orNull(terms.priceUnit),
        notes: orNull(terms.notes),
        commodity: order.value!.mandateId ? null : order.value!.commodity,
        justification: orNull(justification),
      },
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
    router.push(props.policyId && props.mandateId ? paths.mandate(props.policyId, props.mandateId) : paths.exceptions());
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// ---------- Vínculo a posteriori (boleta sem mandato) ----------
const linking = ref(false);
const linkMandates = ref<Mandate[]>([]);
const linkForm = ref({ mandateId: '', justification: '' });
const linkSubmit = useSubmit();

async function openLink() {
  linkSubmit.reset();
  linkForm.value = { mandateId: '', justification: '' };
  linking.value = true;
  const page = await api.mandates.list({ status: 'Active', pageSize: 100 }).catch(() => null);
  const type = order.value!.terms.type === 'Ndf' ? 'Currency' : 'Pricing';
  linkMandates.value = (page?.items ?? []).filter((m) => m.type === type);
  linkForm.value.mandateId = linkMandates.value[0]?.id ?? '';
}

async function link() {
  const updated = await linkSubmit.run(() => api.orders.link(props.orderId, linkForm.value.mandateId, linkForm.value.justification));
  if (updated) {
    linking.value = false;
    applied(updated, `Boleta vinculada ao mandato a posteriori (carimbo permanente).`);
    const m = linkMandates.value.find((x) => x.id === updated.mandateId);
    if (m) router.replace(paths.order(m.policyId, m.id, updated.id));
  }
}

onMounted(load);
</script>

<template>
  <StateBlock :loading="loading && !order" :error="status === 404 ? 'Boleta não encontrada.' : error" @retry="load">
    <template v-if="order">
      <PageHeader
        :kicker="`${order.code} · Boleta de hedge · ${order.mandateCode ? `${order.mandateCode} · ${order.mandateTitle}` : 'sem mandato'}`"
        :title="`${directionLabel[order.terms.direction]} ${orderTypeLabel[order.terms.type]} ${order.terms.tenor}`"
        :subtitle="order.commodity ? `${commodityLabel[order.commodity]} · ${order.counterpartyName}` : `Moeda (US$) · ${order.counterpartyName}`"
      >
        <template #meta>
          <div v-if="stamps.length" class="row stamps">
            <span v-for="s in stamps" :key="s.label" class="badge" :class="`badge-${s.tone}`">{{ s.label }}</span>
          </div>
        </template>
        <template #actions>
          <button v-if="!order.mandateId && order.approval !== 'Rejected' && organization.can(Permission.UpdateOrder)" class="btn btn-primary" @click="openLink">
            Vincular mandato
          </button>
          <template v-if="pendingApproval && organization.can(Permission.ApproveOrder)">
            <button class="btn btn-primary" @click="decision = 'approve'">Aprovar</button>
            <button class="btn btn-danger" @click="decision = 'reject'">Rejeitar</button>
          </template>
          <template v-if="approved && canManageConfirmation && !isExecutor">
            <template v-if="order.confirmation === 'Pending'">
              <button class="btn btn-primary" @click="confirming = true; confirmSubmit.reset()">Confirmação recebida</button>
              <button class="btn btn-danger" @click="decision = 'divergence'">Divergência</button>
              <button class="btn btn-danger" @click="decision = 'refusal'">Recusa</button>
            </template>
            <button v-if="order.confirmation === 'Divergent' || order.confirmation === 'Refused'" class="btn btn-primary" @click="resolve">
              {{ order.confirmation === 'Refused' ? 'Sanar recusa' : 'Sanar divergência' }}
            </button>
          </template>
          <button v-if="editable" class="btn" @click="openEdit">Editar</button>
          <button v-if="!approved && organization.can(Permission.DeleteOrder)" class="btn btn-danger" @click="remove">Excluir</button>
        </template>
      </PageHeader>

      <section class="kpis" style="margin-bottom: 16px">
        <div class="kpi">
          <span>Aprovação</span>
          <strong><StatusBadge :label="approvalLabel[order.approval]" :tone="approvalTone[order.approval]" /></strong>
          <small>{{ order.decidedAt ? `decidida em ${formatDateTime(order.decidedAt)}` : 'sem decisão' }}</small>
        </div>
        <div class="kpi">
          <span>Volume</span>
          <strong>{{ order.terms.type === 'Ndf' ? formatUsd(order.terms.notionalUsd) : formatNumber(order.terms.lots) }}</strong>
          <small>{{ order.terms.type === 'Ndf' ? 'nocional' : 'lotes' }}</small>
        </div>
        <div class="kpi">
          <span>{{ order.terms.type === 'Option' ? 'Strike' : order.terms.type === 'Ndf' ? 'Taxa' : 'Preço' }}</span>
          <strong>{{ formatNumber(order.terms.price, 4) }}</strong>
          <small>{{ order.terms.priceUnit ?? '' }}</small>
        </div>
        <div class="kpi" :class="{ outside: order.compliance.status === 'Outside' }">
          <span>Enquadramento</span>
          <strong>
            <span class="badge" :class="order.compliance.status === 'Outside' ? 'badge-danger' : 'badge-success'">
              {{ order.compliance.status === 'Outside' ? 'FORA' : 'Dentro' }}
            </span>
          </strong>
          <small>{{ order.mandateCode ?? 'sem mandato' }}</small>
        </div>
        <div class="kpi" :class="{ outside: confirmationAttention }">
          <span>Confirmação</span>
          <strong>
            <StatusBadge v-if="approved" :label="confirmationLabel[order.confirmation]" :tone="confirmationTone[order.confirmation]" />
            <span v-else class="muted">—</span>
          </strong>
          <small>{{ order.confirmationOverdue ? 'atrasada (+2 d.u.)' : approved ? 'da contraparte' : 'após a aprovação' }}</small>
        </div>
      </section>

      <nav class="tabs order-tabs" role="tablist" aria-label="Seções da boleta">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="tab" :class="{ active: tab === t.key }" :aria-selected="tab === t.key" @click="selectTab(t.key)">
          {{ t.label }}
          <span v-if="t.alert" class="alert-dot" title="Precisa de atenção" />
        </button>
      </nav>

      <!-- Visão geral: enquadramento e termos da boleta -->
      <div v-if="tab === 'overview'" class="grid-2">
        <section class="card compliance" :class="{ outside: order.compliance.status === 'Outside' }">
          <header class="card-header">
            <h2>Enquadramento</h2>
            <span class="badge" :class="order.compliance.status === 'Outside' ? 'badge-danger' : 'badge-success'">
              {{ order.compliance.status === 'Outside' ? 'FORA' : 'Dentro' }}
            </span>
          </header>
          <div class="card-body stack">
            <p style="margin: 0">{{ order.compliance.reason }}</p>
            <p v-if="order.deviationNote" class="muted" style="margin: 0"><strong>Justificativa:</strong> {{ order.deviationNote }}</p>
            <p v-if="!order.mandateId" class="alert alert-error" style="margin: 0">
              Boleta sem mandato: continua sinalizada até ser vinculada a um mandato — e o vínculo fica carimbado "a posteriori" para sempre.
            </p>
            <p v-else-if="order.linkedAfterExecution" class="muted small" style="margin: 0">Mandato vinculado depois da execução (a posteriori).</p>
          </div>
        </section>

        <section class="card">
          <header class="card-header">
            <h2>Boleta</h2>
            <StatusBadge :label="approvalLabel[order.approval]" :tone="approvalTone[order.approval]" />
          </header>
          <dl class="details card-body">
            <dt>Data da operação</dt>
            <dd>{{ formatDate(order.terms.tradeDate) }}</dd>
            <dt>Contraparte</dt>
            <dd>{{ order.counterpartyName }}</dd>
            <template v-if="order.terms.type === 'Option'">
              <dt>Opção</dt>
              <dd>
                {{ order.terms.optionKind ? optionKindLabel[order.terms.optionKind] : '—' }} · prêmio {{ formatNumber(order.terms.premium, 4) }}
                {{ order.terms.coveredSale ? '· venda coberta' : '' }}
              </dd>
            </template>
            <dt>Observações</dt>
            <dd>{{ order.terms.notes ?? '—' }}</dd>
            <dt>Decisão</dt>
            <dd>{{ order.decidedAt ? `${formatDateTime(order.decidedAt)}${order.decisionNote ? ` · ${order.decisionNote}` : ''}` : '—' }}</dd>
          </dl>
        </section>
      </div>

      <!-- Confirmação da contraparte (middle office) -->
      <section v-else-if="tab === 'confirmation'" class="card">
        <header class="card-header">
          <h2>Confirmação da contraparte</h2>
          <StatusBadge v-if="approved" :label="confirmationLabel[order.confirmation]" :tone="confirmationTone[order.confirmation]" />
        </header>
        <div class="card-body stack">
          <p class="muted small" style="margin: 0">
            Documento da contraparte confirmando os termos da operação, esperado em 2 dias úteis e conferido pelo middle office. Divergente ou
            recusada, a boleta continua no risco: o que trava é a conciliação.
          </p>
          <p v-if="!approved" class="muted" style="margin: 0">A confirmação só se aplica depois da aprovação da boleta.</p>
          <template v-else>
            <p v-if="order.confirmationOverdue" class="alert alert-error">Confirmação pendente há mais de 2 dias úteis.</p>
            <p v-if="isExecutor && canManageConfirmation && order.confirmation !== 'Confirmed'" class="alert alert-info">
              Você executou esta boleta: a conferência da confirmação é de outra pessoa do middle office (segregação).
            </p>
            <dl class="details">
              <dt>Recebida em</dt>
              <dd>{{ formatDate(order.confirmedOn) }}</dd>
              <dt>Observação</dt>
              <dd>{{ order.confirmationNote ?? '—' }}</dd>
            </dl>
          </template>
        </div>
      </section>

      <!-- Auditoria só desta boleta -->
      <template v-else>
        <p class="lead small audit-intro">Tudo o que mudou nesta boleta: registro, enquadramento, decisões, vínculos e confirmação.</p>
        <AuditPanel :key="refreshKey" entity-type="Order" :entity-id="order.id" :scope-label="order.code" />
      </template>
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

  <BaseModal v-if="confirming" title="Confirmação recebida" width="420px" @close="confirming = false">
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
          <option v-for="c in counterparties.filter((x) => x.id !== order?.counterpartyId)" :key="c.id" :value="c.id">{{ c.code }} · {{ c.name }}</option>
        </select>
      </div>
      <OrderTermsForm v-model="editing.terms" :types="orderTypesFor(order.terms.type === 'Ndf' ? 'Currency' : 'Pricing')" :field-error="editSubmit.fieldError" />
      <div class="field">
        <label for="edit-justification">Justificativa do desvio (se houver)</label>
        <textarea id="edit-justification" v-model="editing.justification" class="input" maxlength="500" />
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="editing = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="order-edit-form" :disabled="editSubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>

  <BaseModal v-if="linking && order" title="Vincular mandato (a posteriori)" width="560px" @close="linking = false">
    <form id="link-form" class="stack" @submit.prevent="link">
      <p class="alert alert-info" style="margin: 0">
        O vínculo depois da execução é permitido, mas fica carimbado "a posteriori" para sempre e exige justificativa. O saldo do mandato
        passa a ser consumido; se estourar, a boleta fica FORA.
      </p>
      <p v-if="linkSubmit.error.value" class="alert alert-error">{{ linkSubmit.error.value }}</p>
      <div class="field">
        <label for="link-mandate">Mandato ativo</label>
        <select id="link-mandate" v-model="linkForm.mandateId" class="input" required>
          <option v-for="m in linkMandates" :key="m.id" :value="m.id">
            {{ m.code }} · {{ m.terms.title }} (saldo {{ m.balance === null ? 'sem teto' : formatNumber(m.balance) }})
          </option>
        </select>
        <span v-if="!linkMandates.length" class="field-error">Nenhum mandato ativo compatível com esta boleta.</span>
      </div>
      <div class="field">
        <label for="link-justification">Justificativa</label>
        <textarea id="link-justification" v-model="linkForm.justification" class="input" maxlength="500" required />
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="linking = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="link-form" :disabled="linkSubmit.submitting.value || !linkForm.mandateId">Vincular</button>
    </template>
  </BaseModal>
</template>

<style scoped>
/* Barra de seções em largura total, como na política e no mandato. */
.order-tabs {
  width: 100%;
}

.order-tabs .tab {
  flex: 1 0 auto;
  justify-content: center;
}

.alert-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--danger);
}

.kpi.outside {
  box-shadow: inset 0 3px 0 var(--danger), var(--shadow);
}

.audit-intro {
  margin: 0 0 14px;
}

.stamps {
  gap: 6px;
  margin-top: 6px;
}

.compliance.outside {
  box-shadow: inset 0 3px 0 var(--danger), var(--shadow);
}
</style>