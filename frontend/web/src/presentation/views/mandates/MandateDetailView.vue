<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
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
import AuditPanel from '../../components/audit/AuditPanel.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';

/**
 * Mandato dentro da política (cadeia 1:N), no mesmo formato da política: cabeçalho com indicadores e abas de largura
 * total (Visão geral · Boletas · Auditoria). A aba aberta fica na URL (?tab=).
 */
const props = defineProps<{ policyId: string; mandateId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const { confirm } = useConfirm();
const trail = useTrailStore();

const { data: mandate, loading, error, status, load } = useLoader(async () => {
  const m = await api.mandates.get(props.mandateId);
  // Aberto pela política errada (link antigo/copiado): vai para o caminho certo.
  if (m.policyId !== props.policyId) router.replace(paths.mandate(m.policyId, m.id));
  trail.set(m.policyId, `${m.policyCode.toUpperCase()} ${m.policyVersion}`);
  trail.set(m.id, `${m.code} · ${m.terms.title}`);
  return m;
});
const orders = useLoader(() =>
  organization.can(Permission.ViewOrder) ? api.orders.list({ mandateId: props.mandateId, pageSize: 100 }) : Promise.resolve(null),
);
const refreshKey = ref(0);

// ---------- Abas ----------
type Tab = 'overview' | 'orders' | 'audit';
const tab = computed<Tab>(() => (['orders', 'audit'].includes(route.query.tab as string) ? (route.query.tab as Tab) : 'overview'));
const tabs = computed(() => [
  { key: 'overview' as const, label: 'Visão geral' },
  ...(organization.can(Permission.ViewOrder)
    ? [{ key: 'orders' as const, label: 'Boletas', count: orders.data.value?.totalCount, alert: outsideOrders.value }]
    : []),
  { key: 'audit' as const, label: 'Auditoria' },
]);
const selectTab = (key: Tab) => router.replace({ query: key === 'overview' ? {} : { tab: key } });
/** Boletas deste mandato com desvio (FORA, estouro, a posteriori): contam na aba. */
const outsideOrders = computed(() => orders.data.value?.items.filter((o) => o.compliance.status === 'Outside').length ?? 0);

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
    router.push(paths.policy(props.policyId, 'mandates'));
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
      <PageHeader
        :kicker="`${mandate.code} · Mandato · ${mandateTypeLabel[mandate.type]} · eixo ${mandate.axisCode} · ${mandate.axisTitle}`"
        :title="mandate.terms.title"
        :subtitle="mandate.terms.criteria"
      >
        <template #actions>
          <template v-if="mandate.status === 'PendingApproval'">
            <button v-if="organization.can(Permission.ApproveMandate)" class="btn btn-primary" @click="decision = 'approve'">Aprovar</button>
            <button v-if="organization.can(Permission.ApproveMandate)" class="btn btn-danger" @click="decision = 'reject'">Rejeitar</button>
            <button v-if="organization.can(Permission.UpdateMandate)" class="btn" @click="openEdit">Editar</button>
            <button v-if="organization.can(Permission.DeleteMandate)" class="btn btn-danger" @click="remove">Excluir</button>
          </template>
          <template v-if="mandate.status === 'Active'">
            <template v-if="executable && organization.can(Permission.CreateOrder)">
              <RouterLink class="btn" :to="paths.uploadKind('Orders', true)">Importar boletas</RouterLink>
              <RouterLink class="btn btn-primary" :to="paths.newOrder(props.policyId, mandate.id)">+ Registrar boleta</RouterLink>
            </template>
            <button v-if="organization.can(Permission.UpdateMandate)" class="btn btn-danger" @click="decision = 'close'">Encerrar</button>
          </template>
        </template>
      </PageHeader>

      <section class="kpis" style="margin-bottom: 16px">
        <div class="kpi">
          <span>Status</span>
          <strong><StatusBadge :label="mandateStatusLabel[mandate.status]" :tone="mandateStatusTone[mandate.status]" /></strong>
          <small>{{ mandate.decidedAt ? `decidido em ${formatDateTime(mandate.decidedAt)}` : 'sem decisão' }}</small>
        </div>
        <div class="kpi">
          <span>Autorizado</span>
          <strong>{{ mandate.terms.quantity === null ? 'sem teto' : formatNumber(mandate.terms.quantity) }}</strong>
          <small>{{ unitLabel[mandate.terms.quantityUnit] }}</small>
        </div>
        <div class="kpi">
          <span>Consumido</span>
          <strong>{{ formatNumber(mandate.consumed) }}</strong>
          <small>{{ mandate.terms.quantity ? `${formatNumber(usedPct, 0)}% do autorizado` : 'boletas aprovadas' }}</small>
        </div>
        <div class="kpi">
          <span>Saldo</span>
          <strong>{{ mandate.balance === null ? '—' : formatNumber(mandate.balance) }}</strong>
          <small>disponível para boletas</small>
        </div>
        <div class="kpi" :class="{ outside: mandate.compliance.status === 'Outside' }">
          <span>Enquadramento</span>
          <strong><StatusBadge :label="complianceLabel[mandate.compliance.status]" :tone="complianceTone[mandate.compliance.status]" /></strong>
          <small>política {{ mandate.policyCode }} {{ mandate.policyVersion }}</small>
        </div>
      </section>

      <p v-if="mandate.status === 'PendingApproval' && mandate.compliance.status === 'Outside'" class="alert alert-error">
        Mandato fora da política: a aprovação exige a role approve_exception.
      </p>

      <nav class="tabs mandate-tabs" role="tablist" aria-label="Seções do mandato">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="tab" :class="{ active: tab === t.key }" :aria-selected="tab === t.key" @click="selectTab(t.key)">
          {{ t.label }}
          <span v-if="'count' in t && t.count !== undefined" class="count">{{ t.count }}</span>
          <span v-if="'alert' in t && t.alert" class="alert-count" :title="`${t.alert} boleta(s) FORA`">{{ t.alert }}</span>
        </button>
      </nav>

      <div v-if="tab === 'overview'" class="grid-2">
        <section class="card">
          <header class="card-header">
            <h2>Termos</h2>
            <StatusBadge :label="mandateStatusLabel[mandate.status]" :tone="mandateStatusTone[mandate.status]" />
          </header>
          <dl class="details card-body">
            <dt>Tipo</dt>
            <dd>{{ mandateTypeLabel[mandate.type] }}</dd>
            <dt>Commodity</dt>
            <dd>{{ mandate.terms.commodity ? commodityLabel[mandate.terms.commodity] : "—" }}</dd>
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

      </div>

      <section v-else-if="tab === 'orders'" class="card">
        <header class="card-header">
          <h2>Boletas deste mandato</h2>
          <RouterLink
            v-if="mandate.status === 'Active' && executable && organization.can(Permission.CreateOrder)"
            class="btn btn-primary btn-sm"
            :to="paths.newOrder(props.policyId, mandate.id)"
          >
            + Registrar boleta
          </RouterLink>
        </header>
        <StateBlock :loading="orders.loading.value" :error="orders.error.value" :empty="orders.data.value?.items.length === 0" empty-text="Nenhuma boleta registrada neste mandato." @retry="orders.load">
          <OrderTable :orders="orders.data.value?.items ?? []" :policy-id="props.policyId" />
        </StateBlock>
      </section>

      <template v-else>
        <p class="lead small audit-intro">Tudo o que mudou neste mandato: emissão, enquadramento, decisões e encerramento.</p>
        <AuditPanel :key="refreshKey" entity-type="Mandate" :entity-id="mandate.id" :scope-label="mandate.code" />
      </template>
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
/* Barra de seções em largura total, como na política. */
.mandate-tabs {
  width: 100%;
}

.mandate-tabs .tab {
  flex: 1 0 auto;
  justify-content: center;
}

.alert-count {
  min-width: 18px;
  padding: 0 6px;
  border-radius: 999px;
  background: var(--danger);
  color: #fff;
  font-size: 0.7rem;
  font-weight: 650;
  text-align: center;
}

.audit-intro {
  margin: 0 0 14px;
}

.kpi.outside {
  border-top-color: var(--danger);
}

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
