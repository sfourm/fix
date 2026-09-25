<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import { isPolicyEditable, type Policy } from '@/domain/policy';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatDate, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import MandateTable from '../../components/mandate/MandateTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import PolicyAxesPanel from '../../components/policy/PolicyAxesPanel.vue';
import PolicyBandsPanel from '../../components/policy/PolicyBandsPanel.vue';
import PolicyInstrumentsPanel from '../../components/policy/PolicyInstrumentsPanel.vue';
import PolicyLimitsPanel from '../../components/policy/PolicyLimitsPanel.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import TimelineList from '../../components/TimelineList.vue';

const props = defineProps<{ policyId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const router = useRouter();
const toast = useToast();
const { confirm } = useConfirm();

const { data: policy, loading, error, status, load } = useLoader(() => api.policies.get(props.policyId));
const mandates = useLoader(() =>
  organization.can(Permission.ViewMandate) ? api.mandates.list({ policyId: props.policyId, pageSize: 100 }) : Promise.resolve(null),
);

const tabs = [
  { key: 'limits', label: 'Parâmetros' },
  { key: 'axes', label: 'Eixos' },
  { key: 'bands', label: 'Bandas de cobertura' },
  { key: 'instruments', label: 'Instrumentos' },
  { key: 'mandates', label: 'Mandatos' },
  { key: 'versions', label: 'Versões' },
  { key: 'history', label: 'Histórico' },
] as const;
const tab = ref<(typeof tabs)[number]['key']>('limits');
const refreshKey = ref(0);

const canUpdate = computed(() => organization.can(Permission.UpdatePolicy));
const editable = computed(() => !!policy.value && isPolicyEditable(policy.value) && canUpdate.value);

function onUpdated(updated: Policy) {
  policy.value = updated;
  refreshKey.value++;
}

// ---------- Cabeçalho ----------
const editing = ref(false);
const header = reactive({ code: '', title: '', description: '', validFrom: '', validTo: '' });
const headerSubmit = useSubmit();

function openHeader() {
  const p = policy.value!;
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
  const current = policy.value!.version;
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
    onUpdated(updated);
    lifecycle.value = null;
    toast.success(approving ? 'Política aprovada e vigente. Outras vigentes foram substituídas.' : `Versão ${updated.version} aberta para aprovação.`);
  }
}

async function remove() {
  const ok = await confirm({
    title: 'Excluir política',
    message: `Excluir ${policy.value!.code.toUpperCase()} ${policy.value!.version}? Políticas com mandatos não podem ser excluídas.`,
    confirmLabel: 'Excluir',
    danger: true,
  });
  if (!ok) return;
  try {
    await api.policies.remove(props.policyId);
    toast.success('Política excluída.');
    router.push('/policies');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(() => {
  load();
  mandates.load();
});
</script>

<template>
  <StateBlock :loading="loading && !policy" :error="status === 404 ? 'Política não encontrada.' : error" @retry="load">
    <template v-if="policy">
      <PageHeader :title="`${policy.code.toUpperCase()} · ${policy.title}`" :subtitle="policy.description">
        <template #breadcrumb>
          <nav class="breadcrumb"><RouterLink to="/policies">Política de riscos</RouterLink><span>›</span><span>{{ policy.version }}</span></nav>
        </template>
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

      <section class="card summary">
        <div><span class="muted small">Status</span><StatusBadge :label="policyStatusLabel[policy.status]" :tone="policyStatusTone[policy.status]" /></div>
        <div><span class="muted small">Versão</span><strong>{{ policy.version }}</strong></div>
        <div><span class="muted small">Vigência</span><strong>{{ formatDate(policy.validFrom) }} → {{ formatDate(policy.validTo) }}</strong></div>
        <div><span class="muted small">Ata de aprovação</span><strong>{{ policy.approvalRecord ?? '—' }}</strong></div>
        <div><span class="muted small">Aprovada em</span><strong>{{ formatDate(policy.approvedOn) }}</strong></div>
      </section>

      <nav class="tabs" role="tablist">
        <button v-for="t in tabs" :key="t.key" class="tab" :class="{ active: tab === t.key }" role="tab" @click="tab = t.key">
          {{ t.label }}
          <span v-if="t.key === 'axes'" class="muted">({{ policy.axes.length }})</span>
          <span v-if="t.key === 'mandates' && mandates.data.value" class="muted">({{ mandates.data.value.totalCount }})</span>
        </button>
      </nav>

      <section class="card">
        <PolicyLimitsPanel v-if="tab === 'limits'" :policy="policy" :editable="editable" @updated="onUpdated" />
        <PolicyAxesPanel v-else-if="tab === 'axes'" :policy="policy" :editable="editable" @updated="onUpdated" />
        <PolicyBandsPanel v-else-if="tab === 'bands'" :policy="policy" :editable="editable" @updated="onUpdated" />
        <PolicyInstrumentsPanel v-else-if="tab === 'instruments'" :policy="policy" :editable="editable" @updated="onUpdated" />
        <template v-else-if="tab === 'mandates'">
          <div v-if="organization.can(Permission.CreateMandate) && policy.status === 'Active'" class="card-body" style="padding-bottom: 0">
            <RouterLink class="btn btn-primary btn-sm" :to="`/mandates/new?policyId=${policy.id}`">+ Emitir mandato</RouterLink>
          </div>
          <StateBlock :loading="mandates.loading.value" :error="mandates.error.value" :empty="mandates.data.value?.items.length === 0" empty-text="Nenhum mandato emitido nesta política." @retry="mandates.load">
            <MandateTable :mandates="mandates.data.value?.items ?? []" />
          </StateBlock>
        </template>
        <div v-else-if="tab === 'versions'" class="table-wrap">
          <table class="table">
            <thead><tr><th>Versão</th><th>Status</th><th>Data</th><th>Nota</th></tr></thead>
            <tbody>
              <tr v-for="(v, i) in policy.versions" :key="`${v.version}-${i}`">
                <td><strong>{{ v.version }}</strong></td>
                <td><StatusBadge :label="policyStatusLabel[v.status]" :tone="policyStatusTone[v.status]" /></td>
                <td>{{ formatDate(v.date) }}</td>
                <td class="muted">{{ v.note ?? '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <TimelineList v-else entity-type="Policy" :entity-id="policy.id" :refresh-key="refreshKey" :limit="100" />
      </section>
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
.summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 12px 20px;
  padding: 16px 20px;
  margin-bottom: 16px;
}

.summary > div {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
}
</style>
