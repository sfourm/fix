<script setup lang="ts">
import { computed, onMounted, provide, reactive, ref, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import { isPolicyEditable, type Policy } from '@/domain/policy';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatDate, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';
import { policyContextKey, workCounts, type PolicyWork } from './policy-context';

/**
 * Tela da política: raiz da cadeia 1:N. Cabeçalho, indicadores e abas; mandatos, aprovações e confirmações
 * desta política ficam aqui dentro (as abas recebem a política e o trabalho em aberto por injeção).
 */
const props = defineProps<{ policyId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const trail = useTrailStore();
const route = useRoute();
const router = useRouter();
const toast = useToast();
const { confirm } = useConfirm();

const { data: loaded, loading, error, status, load } = useLoader(async () => {
  const policy = await api.policies.get(props.policyId);
  trail.set(policy.id, `${policy.code.toUpperCase()} ${policy.version}`);
  return policy;
});
const policy = computed(() => loaded.value) as Ref<Policy>;
const refreshKey = ref(0);

const canUpdate = computed(() => organization.can(Permission.UpdatePolicy));
const editable = computed(() => !!loaded.value && isPolicyEditable(loaded.value) && canUpdate.value);

// ---------- Trabalho em aberto da política (mandatos, aprovações, confirmações) ----------
const work = ref<PolicyWork | null>(null);

async function reloadWork() {
  const canMandates = organization.can(Permission.ViewMandate);
  const canOrders = organization.can(Permission.ViewOrder);
  const mandates = canMandates ? (await api.mandates.list({ policyId: props.policyId, pageSize: 100 })).items : [];
  const ids = new Set(mandates.map((m) => m.id));
  const mine = <T extends { mandateId: string | null }>(items: T[]) => items.filter((o) => o.mandateId !== null && ids.has(o.mandateId));
  const [pendingOrders, awaiting, divergent, refused] = canOrders && ids.size
    ? await Promise.all([
        api.orders.list({ approval: 'PendingApproval', pageSize: 100 }),
        api.orders.list({ approval: 'Approved', confirmation: 'Pending', pageSize: 100 }),
        api.orders.list({ confirmation: 'Divergent', pageSize: 100 }),
        api.orders.list({ confirmation: 'Refused', pageSize: 100 }),
      ]).then((pages) => pages.map((p) => mine(p.items)))
    : [[], [], [], []];
  work.value = {
    mandates,
    pendingMandates: mandates.filter((m) => m.status === 'PendingApproval'),
    pendingOrders: pendingOrders!,
    awaitingConfirmation: awaiting!,
    confirmationProblems: [...divergent!, ...refused!],
  };
  queue.refresh();
}

const counts = workCounts(work);

function onUpdated(updated: Policy) {
  loaded.value = updated;
  trail.set(updated.id, `${updated.code.toUpperCase()} ${updated.version}`);
  refreshKey.value++;
}

provide(policyContextKey, { policy, editable, work, refreshKey, onUpdated, reloadWork });

const tabs = computed(() => [
  { to: paths.policy(props.policyId), label: 'Visão geral', exact: true },
  { to: paths.policy(props.policyId, 'axes'), label: 'Eixos & bandas', count: loaded.value?.axes.length },
  { to: paths.policy(props.policyId, 'instruments'), label: 'Instrumentos' },
  ...(organization.can(Permission.ViewMandate)
    ? [{ to: paths.policy(props.policyId, 'mandates'), label: 'Mandatos', count: work.value?.mandates.length }]
    : []),
  { to: paths.policy(props.policyId, 'approvals'), label: 'Aprovações', alert: counts.approvals.value },
  ...(organization.can(Permission.ViewOrder)
    ? [{ to: paths.policy(props.policyId, 'confirmations'), label: 'Confirmações', alert: counts.confirmations.value }]
    : []),
  { to: paths.policy(props.policyId, 'history'), label: 'Versões' },
  { to: paths.policy(props.policyId, 'audit'), label: 'Auditoria' },
]);
const isActive = (tab: { to: string; exact?: boolean }) => (tab.exact ? route.path === tab.to : route.path.startsWith(tab.to));

const activeMandates = computed(() => work.value?.mandates.filter((m) => m.status === 'Active').length ?? 0);

// ---------- Cabeçalho ----------
const editing = ref(false);
const header = reactive({ code: '', title: '', description: '', validFrom: '', validTo: '' });
const headerSubmit = useSubmit();

function openHeader() {
  const p = policy.value;
  Object.assign(header, { code: p.code, title: p.title, description: p.description ?? '', validFrom: p.validFrom, validTo: p.validTo ?? '' });
  headerSubmit.reset();
  editing.value = true;
}

async function saveHeader() {
  const updated = await headerSubmit.run(() =>
    api.policies.update(props.policyId, { ...header, description: orNull(header.description), validTo: orNull(header.validTo) }),
  );
  if (updated) {
    onUpdated(updated);
    editing.value = false;
    toast.success('Política atualizada.');
  }
}

// ---------- Ciclo de aprovação ----------
const lifecycle = ref<'approve' | 'version' | null>(null);
const lifecycleForm = reactive({ approvalRecord: '', version: '', reason: '' });
const lifecycleSubmit = useSubmit();

async function submitForApproval() {
  const ok = await confirm({
    title: 'Enviar para aprovação',
    message: 'A política segue editável em aprovação, mas só passa a valer depois de aprovada com o número da ata do Conselho.',
    confirmLabel: 'Enviar',
  });
  if (!ok) return;
  try {
    onUpdated(await api.policies.submit(props.policyId));
    toast.success('Política enviada para aprovação.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

function openLifecycle(kind: 'approve' | 'version') {
  const current = policy.value.version;
  const next = current.replace(/(\d+)\.(\d+)$/, (_, major: string, minor: string) => `${major}.${Number(minor) + 1}`);
  Object.assign(lifecycleForm, { approvalRecord: '', version: next === current ? `${current}.1` : next, reason: '' });
  lifecycleSubmit.reset();
  lifecycle.value = kind;
}

async function confirmLifecycle() {
  const approving = lifecycle.value === 'approve';
  const updated = await lifecycleSubmit.run(() =>
    approving
      ? api.policies.approve(props.policyId, lifecycleForm.approvalRecord)
      : api.policies.openVersion(props.policyId, { version: lifecycleForm.version, reason: orNull(lifecycleForm.reason) }),
  );
  if (updated) {
    lifecycle.value = null;
    toast.success(approving ? 'Política aprovada e vigente. Outras vigentes foram substituídas.' : `Versão ${updated.version} aberta para aprovação.`);
    // Nova versão é outra política: navega para ela.
    if (updated.id !== props.policyId) router.push(paths.policy(updated.id));
    else onUpdated(updated);
  }
}

async function remove() {
  const ok = await confirm({
    title: 'Excluir política',
    message: `Excluir ${policy.value.code.toUpperCase()} ${policy.value.version}? Políticas com mandatos não podem ser excluídas.`,
    confirmLabel: 'Excluir',
    danger: true,
  });
  if (!ok) return;
  try {
    await api.policies.remove(props.policyId);
    toast.success('Política excluída.');
    router.push(paths.policies());
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(async () => {
  await load();
  if (loaded.value) reloadWork().catch((e) => toast.error(errorMessage(e)));
});
</script>

<template>
  <StateBlock :loading="loading && !loaded" :error="status === 404 ? 'Política não encontrada.' : error" @retry="load">
    <template v-if="loaded">
      <PageHeader :kicker="`Política-mãe · ${policy.code} · ${policy.version}`" :title="policy.title" :subtitle="policy.description">
        <template #actions>
          <button v-if="editable" class="btn" @click="openHeader">Editar</button>
          <button v-if="canUpdate && policy.status === 'Draft'" class="btn btn-primary" @click="submitForApproval">Enviar para aprovação</button>
          <button v-if="organization.can(Permission.ApprovePolicy) && policy.status === 'UnderApproval'" class="btn btn-primary" @click="openLifecycle('approve')">
            Aprovar (ata)
          </button>
          <button v-if="organization.can(Permission.ApprovePolicy) && policy.status === 'Active'" class="btn" @click="openLifecycle('version')">
            Abrir nova versão
          </button>
          <button v-if="organization.can(Permission.DeletePolicy) && policy.status !== 'Active'" class="btn btn-danger" @click="remove">Excluir</button>
        </template>
      </PageHeader>

      <section class="kpis">
        <div class="kpi">
          <span>Versão · status</span>
          <strong class="row" style="gap: 10px">{{ policy.version }} <StatusBadge :label="policyStatusLabel[policy.status]" :tone="policyStatusTone[policy.status]" /></strong>
          <small>{{ policy.approvalRecord ? `ata ${policy.approvalRecord}` : 'sem ata de aprovação' }}</small>
        </div>
        <div class="kpi">
          <span>Vigência</span>
          <strong class="compact">{{ formatDate(policy.validFrom) }} → {{ policy.validTo ? formatDate(policy.validTo) : '…' }}</strong>
          <small>{{ policy.approvedOn ? `aprovada em ${formatDate(policy.approvedOn)}` : 'aguarda aprovação' }}</small>
        </div>
        <div class="kpi">
          <span>Eixos</span>
          <strong>{{ policy.axes.length }}</strong>
          <small>{{ policy.bands.length }} banda(s) · {{ policy.instruments.length }} instrumento(s)</small>
        </div>
        <RouterLink v-if="organization.can(Permission.ViewMandate)" class="kpi click" :to="paths.policy(policy.id, 'mandates')">
          <span>Mandatos</span>
          <strong>{{ work?.mandates.length ?? '…' }}</strong>
          <small>{{ activeMandates }} ativo(s)</small>
        </RouterLink>
        <RouterLink class="kpi click" :class="{ attention: counts.approvals.value + counts.confirmations.value > 0 }" :to="paths.policy(policy.id, counts.approvals.value ? 'approvals' : 'confirmations')">
          <span>Em aberto</span>
          <strong>{{ work ? counts.approvals.value + counts.confirmations.value : '…' }}</strong>
          <small>{{ counts.approvals.value }} aguardando aprovação · {{ counts.confirmations.value }} confirmação(ões) a conciliar</small>
        </RouterLink>
      </section>

      <nav class="tabs policy-tabs" aria-label="Seções da política">
        <RouterLink v-for="t in tabs" :key="t.to" :to="t.to" class="tab" :class="{ active: isActive(t) }">
          {{ t.label }}
          <span v-if="t.count !== undefined" class="count">{{ t.count }}</span>
          <span v-if="t.alert" class="alert-count">{{ t.alert }}</span>
        </RouterLink>
      </nav>

      <RouterView />
    </template>
  </StateBlock>

  <BaseModal v-if="editing" title="Editar política" @close="editing = false">
    <form id="policy-header-form" class="stack" @submit.prevent="saveHeader">
      <p v-if="headerSubmit.error.value" class="alert alert-error">{{ headerSubmit.error.value }}</p>
      <div class="field">
        <label for="header-code">Código</label>
        <input id="header-code" v-model="header.code" class="input" maxlength="32" required />
      </div>
      <div class="field">
        <label for="header-title">Título</label>
        <input id="header-title" v-model="header.title" class="input" maxlength="200" required />
      </div>
      <div class="form-grid">
        <div class="field">
          <label for="header-from">Vigência de</label>
          <input id="header-from" v-model="header.validFrom" class="input" type="date" required />
        </div>
        <div class="field">
          <label for="header-to">até</label>
          <input id="header-to" v-model="header.validTo" class="input" type="date" />
        </div>
      </div>
      <div class="field">
        <label for="header-description">Descrição</label>
        <textarea id="header-description" v-model="header.description" class="input" maxlength="2000" />
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="editing = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="policy-header-form" :disabled="headerSubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>

  <BaseModal v-if="lifecycle" :title="lifecycle === 'approve' ? 'Aprovar política' : 'Abrir nova versão'" width="480px" @close="lifecycle = null">
    <form id="lifecycle-form" class="stack" @submit.prevent="confirmLifecycle">
      <p v-if="lifecycleSubmit.error.value" class="alert alert-error">{{ lifecycleSubmit.error.value }}</p>
      <template v-if="lifecycle === 'approve'">
        <p style="margin: 0">A política passa a vigente e substitui a versão vigente anterior.</p>
        <div class="field">
          <label for="approval-record">Nº/data da ata do Conselho</label>
          <input id="approval-record" v-model="lifecycleForm.approvalRecord" class="input" maxlength="200" required autofocus />
        </div>
      </template>
      <template v-else>
        <p style="margin: 0">A versão vigente continua valendo até a nova ser aprovada.</p>
        <div class="field">
          <label for="new-version">Nova versão</label>
          <input id="new-version" v-model="lifecycleForm.version" class="input" maxlength="16" required />
        </div>
        <div class="field">
          <label for="version-reason">Motivo da revisão</label>
          <textarea id="version-reason" v-model="lifecycleForm.reason" class="input" maxlength="1000" />
        </div>
      </template>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="lifecycle = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="lifecycle-form" :disabled="lifecycleSubmit.submitting.value">
        {{ lifecycle === 'approve' ? 'Aprovar' : 'Abrir versão' }}
      </button>
    </template>
  </BaseModal>
</template>

<style scoped>
.kpis {
  margin-bottom: 18px;
}

.kpi.click {
  color: inherit;
  transition: background 0.15s;
}

.kpi.click:hover {
  background: var(--surface-2);
}

.kpi.attention {
  border-top-color: var(--danger);
}

.kpi.attention strong {
  color: var(--danger);
}

/* Barra de seções: largura total, abas dividem o espaço com o texto centralizado (rola se não couber). */
.policy-tabs {
  width: 100%;
  margin-bottom: 18px;
}

.policy-tabs .tab {
  flex: 1 0 auto;
  justify-content: center;
}

.alert-count {
  min-width: 18px;
  padding: 0 6px;
  border-radius: 999px;
  background: var(--danger);
  color: #fff;
  font-variant-numeric: tabular-nums;
  font-weight: 650;
  font-size: 0.7rem;
  text-align: center;
}
</style>
