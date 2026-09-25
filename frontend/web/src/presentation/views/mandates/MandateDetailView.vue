<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import {
  commodityLabel,
  complianceLabel,
  complianceTone,
  mandateStatusLabel,
  mandateStatusTone,
  mandateTypeLabel,
  unitLabel,
} from '@/domain/labels';
import type { Mandate, MandateTerms } from '@/domain/mandate';
import { orderTypesFor } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatDate, formatDateTime, formatNumber, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import DecisionModal from '../../components/DecisionModal.vue';
import MandateTermsForm from '../../components/mandate/MandateTermsForm.vue';
import OrderTable from '../../components/order/OrderTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import TimelineList from '../../components/TimelineList.vue';

const props = defineProps<{ mandateId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const toast = useToast();
const { confirm } = useConfirm();

const { data: mandate, loading, error, status, load } = useLoader(() => api.mandates.get(props.mandateId));
const orders = useLoader(() =>
  organization.can(Permission.ViewOrder) ? api.orders.list({ mandateId: props.mandateId, pageSize: 100 }) : Promise.resolve(null),
);
const refreshKey = ref(0);

const executable = computed(() => !!mandate.value && orderTypesFor(mandate.value.type).length > 0);
const usedPct = computed(() => {
  const m = mandate.value;
  return m?.terms.quantity ? Math.min(100, (m.consumed / m.terms.quantity) * 100) : 0;
});

function price(terms: MandateTerms): string {
  const p = terms.price;
  if (p.atMarket) return 'a mercado';
  const parts = [p.min !== null && `mín ${formatNumber(p.min)}`, p.target !== null && `target ${formatNumber(p.target)}`, p.max !== null && `máx ${formatNumber(p.max)}`];
  const text = parts.filter(Boolean).join(' · ');
  return text ? `${text} ${p.unit ?? ''}` : '—';
}

function applied(updated: Mandate, message: string) {
  mandate.value = updated;
  refreshKey.value++;
  queue.refresh();
  toast.success(message);
}

// ---------- Decisões ----------
type Decision = 'approve' | 'reject' | 'close';
const decision = ref<Decision | null>(null);
const decisions: Record<Decision, { title: string; label: string; required: boolean; danger?: boolean; message: string }> = {
  approve: { title: 'Aprovar mandato', label: 'Aprovar', required: false, message: 'O mandato passa a ativo e aceita boletas até o saldo autorizado.' },
  reject: { title: 'Rejeitar mandato', label: 'Rejeitar', required: true, danger: true, message: 'A rejeição exige justificativa.' },
  close: { title: 'Encerrar mandato', label: 'Encerrar', required: false, danger: true, message: 'Mandatos encerrados não aceitam novas boletas.' },
};

async function decide(note: string | null) {
  const kind = decision.value!;
  const updated =
    kind === 'approve'
      ? await api.mandates.approve(props.mandateId, note)
      : kind === 'reject'
        ? await api.mandates.reject(props.mandateId, note ?? '')
        : await api.mandates.close(props.mandateId, note);
  decision.value = null;
  applied(updated, kind === 'approve' ? 'Mandato aprovado.' : kind === 'reject' ? 'Mandato rejeitado.' : 'Mandato encerrado.');
}

// ---------- Edição (somente pendentes) ----------
const editing = ref<MandateTerms | null>(null);
const editSubmit = useSubmit();

function openEdit() {
  editing.value = structuredClone({ ...mandate.value!.terms, price: { ...mandate.value!.terms.price } });
  editSubmit.reset();
}

async function saveEdit() {
  const t = editing.value!;
  const updated = await editSubmit.run(() =>
    api.mandates.update(props.mandateId, {
      ...t,
      criteria: orNull(t.criteria),
      tenor: orNull(t.tenor),
      windowStart: orNull(t.windowStart),
      windowEnd: orNull(t.windowEnd),
      price: { ...t.price, unit: orNull(t.price.unit) },
    }),
  );
  if (updated) {
    editing.value = null;
    applied(updated, 'Mandato atualizado e reenquadrado.');
  }
}

async function remove() {
  const ok = await confirm({ title: 'Excluir mandato', message: 'Mandatos com boletas não podem ser excluídos.', confirmLabel: 'Excluir', danger: true });
  if (!ok) return;
  try {
    await api.mandates.remove(props.mandateId);
    queue.refresh();
    toast.success('Mandato excluído.');
    router.push('/mandates');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(() => {
  load();
  orders.load();
});
</script>

<template>
  <StateBlock :loading="loading && !mandate" :error="status === 404 ? 'Mandato não encontrado.' : error" @retry="load">
    <template v-if="mandate">
      <PageHeader :title="mandate.terms.title" :subtitle="mandate.terms.criteria">
        <template #breadcrumb>
          <nav class="breadcrumb">
            <RouterLink to="/mandates">Mandatos</RouterLink><span>›</span>
            <RouterLink :to="`/policies/${mandate.policyId}`">{{ mandate.policyCode.toUpperCase() }} {{ mandate.policyVersion }}</RouterLink>
            <span>›</span><span>{{ mandate.axisCode }} · {{ mandate.axisTitle }}</span>
          </nav>
        </template>
        <template #actions>
          <template v-if="mandate.status === 'PendingApproval'">
            <button v-if="organization.can(Permission.ApproveMandate)" class="btn btn-primary" @click="decision = 'approve'">Aprovar</button>
            <button v-if="organization.can(Permission.ApproveMandate)" class="btn btn-danger" @click="decision = 'reject'">Rejeitar</button>
            <button v-if="organization.can(Permission.UpdateMandate)" class="btn" @click="openEdit">Editar</button>
            <button v-if="organization.can(Permission.DeleteMandate)" class="btn btn-danger" @click="remove">Excluir</button>
          </template>
          <template v-if="mandate.status === 'Active'">
            <RouterLink v-if="executable && organization.can(Permission.CreateOrder)" class="btn btn-primary" :to="`/orders/new?mandateId=${mandate.id}`">
              + Registrar boleta
            </RouterLink>
            <button v-if="organization.can(Permission.UpdateMandate)" class="btn btn-danger" @click="decision = 'close'">Encerrar</button>
          </template>
        </template>
      </PageHeader>

      <p v-if="mandate.status === 'PendingApproval' && mandate.compliance.status === 'Outside'" class="alert alert-error">
        Mandato fora da política: a aprovação exige a role approve_exception.
      </p>

      <div class="grid-2">
        <section class="card">
          <header class="card-header">
            <h2>Termos</h2>
            <StatusBadge :label="mandateStatusLabel[mandate.status]" :tone="mandateStatusTone[mandate.status]" />
          </header>
          <dl class="details card-body">
            <dt>Tipo</dt>
            <dd>{{ mandateTypeLabel[mandate.type] }}</dd>
            <dt>Commodity</dt>
            <dd>{{ commodityLabel[mandate.terms.commodity] }}</dd>
            <dt>Tela</dt>
            <dd>{{ mandate.terms.tenor ?? '—' }}</dd>
            <dt>Preço</dt>
            <dd>{{ price(mandate.terms) }}</dd>
            <dt>Janela</dt>
            <dd>{{ formatDate(mandate.terms.windowStart) }} → {{ formatDate(mandate.terms.windowEnd) }}</dd>
            <dt>Decisão</dt>
            <dd>{{ mandate.decidedAt ? `${formatDateTime(mandate.decidedAt)}${mandate.decisionNote ? ` · ${mandate.decisionNote}` : ''}` : '—' }}</dd>
          </dl>
        </section>

        <section class="card">
          <header class="card-header">
            <h2>Enquadramento e saldo</h2>
            <StatusBadge :label="complianceLabel[mandate.compliance.status]" :tone="complianceTone[mandate.compliance.status]" />
          </header>
          <div class="card-body stack">
            <p style="margin: 0">{{ mandate.compliance.reason }}</p>
            <template v-if="mandate.terms.quantity !== null">
              <div class="bar" role="progressbar" :aria-valuenow="usedPct" aria-valuemin="0" aria-valuemax="100">
                <span :style="{ width: `${usedPct}%` }" />
              </div>
              <div class="row" style="justify-content: space-between">
                <span>Consumido <strong>{{ formatNumber(mandate.consumed) }}</strong></span>
                <span>Saldo <strong>{{ formatNumber(mandate.balance) }}</strong> de {{ formatNumber(mandate.terms.quantity) }} {{ unitLabel[mandate.terms.quantityUnit] }}</span>
              </div>
              <p class="muted small" style="margin: 0">Só boletas aprovadas consomem saldo.</p>
            </template>
            <p v-else class="muted" style="margin: 0">Mandato sem teto de volume.</p>
          </div>
        </section>

        <section v-if="organization.can(Permission.ViewOrder)" class="card" style="grid-column: 1 / -1">
          <header class="card-header"><h2>Boletas</h2></header>
          <StateBlock :loading="orders.loading.value" :error="orders.error.value" :empty="orders.data.value?.items.length === 0" empty-text="Nenhuma boleta registrada neste mandato." @retry="orders.load">
            <OrderTable :orders="orders.data.value?.items ?? []" />
          </StateBlock>
        </section>

        <section class="card" style="grid-column: 1 / -1">
          <header class="card-header"><h2>Histórico</h2></header>
          <TimelineList entity-type="Mandate" :entity-id="mandate.id" :refresh-key="refreshKey" />
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
    :danger="decisions[decision].danger"
    :action="decide"
    @close="decision = null"
  />

  <BaseModal v-if="editing && mandate" title="Editar mandato" width="760px" @close="editing = null">
    <form id="mandate-edit-form" class="stack" @submit.prevent="saveEdit">
      <p v-if="editSubmit.error.value" class="alert alert-error">{{ editSubmit.error.value }}</p>
      <MandateTermsForm v-model="editing" :type="mandate.type" :field-error="editSubmit.fieldError" />
    </form>
    <template #footer>
      <button class="btn" type="button" @click="editing = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="mandate-edit-form" :disabled="editSubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.bar {
  height: 10px;
  border-radius: 999px;
  background: var(--surface-2);
  overflow: hidden;
}

.bar span {
  display: block;
  height: 100%;
  background: var(--primary);
}
</style>
