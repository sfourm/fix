<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import type { Policy } from '@/domain/policy';
import type { TimelineEntry } from '@/domain/timeline';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';
import { formatDate, formatDateTime, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import PolicyVersionSheet from '../../components/policy/PolicyVersionSheet.vue';
import { timedVersions, type TimedVersion } from '../../components/policy/policy-changes';
import { paths } from '../../paths';

const api = useApi();
const organization = useOrganizationStore();
const router = useRouter();
const toast = useToast();

const page = ref(1);
const { data, loading, error, load: loadList } = useLoader(() => api.policies.list({ page: page.value, pageSize: 10 }));

/** Detalhe de cada política da página (traz as versões mostradas no card). */
const details = ref<Record<string, Policy>>({});
/** Auditoria de cada política: dá o horário das etapas das versões. */
const audits = ref<Record<string, TimelineEntry[]>>({});
async function load() {
  await loadList();
  const items = data.value?.items ?? [];
  const [loaded, trails] = await Promise.all([
    Promise.all(items.map((p) => api.policies.get(p.id).catch(() => null))),
    Promise.all(items.map((p) => api.timeline.list({ entityType: 'Policy', entityId: p.id, limit: 500 }).catch(() => []))),
  ]);
  details.value = Object.fromEntries(loaded.filter((p): p is Policy => !!p).map((p) => [p.id, p]));
  audits.value = Object.fromEntries(items.map((p, i) => [p.id, trails[i] ?? []]));
}

/** Versões únicas de uma política, cada uma com a última etapa (status, data e hora). */
function versionsOf(id: string) {
  const list = timedVersions(details.value[id]?.versions ?? [], audits.value[id]);
  const last = new Map<string, TimedVersion>();
  list.forEach((v) => last.set(v.version, v));
  // Versões anteriores à atual já foram substituídas (o histórico só grava até a vigência).
  const current = details.value[id]?.version;
  return [...last.values()].map((v) => (v.version !== current && v.status === 'Active' ? { ...v, status: 'Superseded' as const } : v));
}

const when = (v: TimedVersion) => (v.at ? formatDateTime(v.at) : formatDate(v.date));
const versionTitle = (v: TimedVersion) =>
  `${policyStatusLabel[v.status]} · ${when(v)}${v.note ? ` · ${v.note}` : ''} — clique para ver as alterações`;

const sheet = ref<{ policyId: string; version: string } | null>(null);
const sheetPolicy = computed(() => (sheet.value ? details.value[sheet.value.policyId] ?? null : null));


const year = new Date().getFullYear();
const creating = ref(false);
const form = reactive({
  code: `pol-${year}`,
  title: 'Política de gestão de riscos de mercado',
  version: 'v1.0',
  description: '',
  validFrom: `${year}-01-01`,
  validTo: `${year + 1}-12-31`,
  useTemplate: true,
});
const submit = useSubmit();

async function create() {
  const policy = await submit.run(() =>
    api.policies.create({ ...form, description: orNull(form.description), validTo: orNull(form.validTo) }),
  );
  if (policy) {
    toast.success('Política criada em rascunho.');
    router.push(`/policies/${policy.id}`);
  }
}

function goTo(p: number) {
  page.value = p;
  load();
}

onMounted(load);
</script>

<template>
  <PageHeader
    title="Política de riscos"
    subtitle="Política-mãe versionada: limites, eixos por fator de risco, bandas de cobertura e instrumentos. Só vale depois de aprovada em ata."
  >
    <template #actions>
      <button v-if="organization.can(Permission.CreatePolicy)" class="btn btn-primary" @click="creating = true; submit.reset()">+ Nova política</button>
    </template>
  </PageHeader>

  <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhuma política cadastrada ainda." @retry="load">
    <div class="policy-grid">
      <article v-for="policy in data?.items ?? []" :key="policy.id" class="card policy-card" @click="router.push(paths.policy(policy.id))">
        <header class="pc-head">
          <span class="kicker">{{ policy.code.toUpperCase() }}</span>
          <StatusBadge :label="policyStatusLabel[policy.status]" :tone="policyStatusTone[policy.status]" />
        </header>
        <h2>{{ policy.title }}</h2>
        <p class="muted small pc-meta">
          <span class="num">{{ formatDate(policy.validFrom) }} → {{ formatDate(policy.validTo) }}</span>
          <span>·</span>
          <span>{{ policy.axesCount }} eixo(s)</span>
        </p>

        <div class="pc-versions">
          <span class="pc-label">Versões</span>
          <div class="vlist">
            <button
              v-for="v in versionsOf(policy.id)"
              :key="v.version"
              type="button"
              class="vpill"
              :class="['tone-' + policyStatusTone[v.status], { current: v.version === policy.version }]"
              :title="versionTitle(v)"
              @click.stop="sheet = { policyId: policy.id, version: v.version }"
            >
              <span class="vdot" />
              <strong>{{ v.version }}</strong>
              <span class="vstatus">{{ policyStatusLabel[v.status] }} · {{ when(v) }}</span>
            </button>
            <span v-if="!details[policy.id]" class="muted small">carregando…</span>
          </div>
        </div>

        <footer class="pc-foot">
          <span class="muted small">Clique numa versão para ver o que mudou</span>
          <span class="open">Abrir →</span>
        </footer>
      </article>
    </div>
    <PaginationBar v-if="data && data.totalCount > data.pageSize" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
  </StateBlock>

  <PolicyVersionSheet
    v-if="sheet && sheetPolicy"
    :policy="sheetPolicy"
    :version="sheet.version"
    @select="(v) => sheet && (sheet = { ...sheet, version: v })"
    @close="sheet = null"
  />

  <BaseModal v-if="creating" title="Nova política" @close="creating = false">
    <form id="policy-form" class="stack" @submit.prevent="create">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <div class="form-grid">
        <div class="field">
          <label for="policy-code">Código</label>
          <input id="policy-code" v-model="form.code" class="input" maxlength="32" required />
        </div>
        <div class="field">
          <label for="policy-version">Versão</label>
          <input id="policy-version" v-model="form.version" class="input" maxlength="16" required />
        </div>
      </div>
      <div class="field">
        <label for="policy-title">Título</label>
        <input id="policy-title" v-model="form.title" class="input" maxlength="200" required />
      </div>
      <div class="form-grid">
        <div class="field">
          <label for="policy-from">Vigência de</label>
          <input id="policy-from" v-model="form.validFrom" class="input" type="date" required />
        </div>
        <div class="field">
          <label for="policy-to">até</label>
          <input id="policy-to" v-model="form.validTo" class="input" type="date" />
          <span v-if="submit.fieldError('validTo')" class="field-error">{{ submit.fieldError('validTo') }}</span>
        </div>
      </div>
      <div class="field">
        <label for="policy-description">Descrição</label>
        <textarea id="policy-description" v-model="form.description" class="input" maxlength="2000" />
      </div>
      <label class="row"><input v-model="form.useTemplate" type="checkbox" /> Iniciar com o modelo FIX (eixos, bandas de cobertura e instrumentos)</label>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="creating = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="policy-form" :disabled="submit.submitting.value">Criar rascunho</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.policy-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 380px), 1fr));
  gap: 16px;
}

.policy-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 20px 22px 16px;
  cursor: pointer;
}

.policy-card:hover {
  box-shadow: var(--shadow-hover);
  transform: translateY(-2px);
}

.policy-card {
  transition:
    box-shadow 0.3s var(--ease),
    transform 0.3s var(--ease);
}

.pc-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.policy-card h2 {
  font-size: 1.25rem;
}

.pc-meta {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
  margin: 0;
}

.pc-versions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 8px;
  padding-top: 14px;
  border-top: 1px solid var(--border);
}

.pc-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-muted);
}

.vlist {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.vpill {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 7px 13px 7px 11px;
  border: 1px solid transparent;
  border-radius: 999px;
  background: var(--fill);
  color: var(--text);
  font: inherit;
  font-size: 0.86rem;
  cursor: pointer;
  transition:
    background 0.2s var(--ease),
    border-color 0.2s var(--ease),
    transform 0.2s var(--ease);
}

.vpill:hover {
  background: var(--fill-strong);
}

.vpill:active {
  transform: scale(0.96);
}

.vpill.current {
  border-color: var(--primary-line);
  background: var(--primary-soft);
}

.vpill strong {
  font-variant-numeric: tabular-nums;
}

.vstatus {
  color: var(--text-muted);
  font-size: 0.78rem;
}

.vdot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--text-faint);
}

.tone-success .vdot {
  background: var(--success);
}

.tone-warning .vdot {
  background: var(--warning);
}

.tone-info .vdot {
  background: var(--info);
}

.pc-foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-top: 6px;
}

.open {
  color: var(--primary);
  font-size: 0.88rem;
  font-weight: 600;
}
</style>
