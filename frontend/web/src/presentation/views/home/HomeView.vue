<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { VueDraggable } from 'vue-draggable-plus';
import { useApi } from '@/application/api-provider';
import { useCatalogStore } from '@/application/stores/catalog.store';
import { useOrganizationStore } from '@/application/stores/organization.store';
import type { Dashboard, DashboardSummary, Widget } from '@/domain/dashboard';
import { sectorLabel } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import { visibilityLabel, VISIBILITIES, type Visibility } from '@/domain/search';
import type { DashboardTemplate } from '@/infrastructure/api/dashboard.api';
import { errorMessage } from '@/infrastructure/http/api-error';
import { storage } from '@/infrastructure/storage/local-storage';
import { useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import WidgetCard from '../../components/dashboard/WidgetCard.vue';
import WidgetEditorModal from '../../components/dashboard/WidgetEditorModal.vue';

const api = useApi();
const organization = useOrganizationStore();
const catalog = useCatalogStore();
const toast = useToast();
const { confirm } = useConfirm();
const setup = computed(() => organization.current);

// ---------- Checklist de configuração (some quando tudo estiver pronto) ----------
const progress = reactive({ activePolicy: false, activeMandate: false, loaded: false });
const checklist = computed(() => [
  { done: !!setup.value?.profile.activeCrop, label: 'Identificação e safra ativa', to: '/setup' },
  { done: (setup.value?.commodities.length ?? 0) > 0, label: 'Commodities e capacidade', to: '/setup' },
  { done: setup.value?.budget.economicFloor != null, label: 'Orçamento e gatilhos (piso econômico)', to: '/setup' },
  { done: progress.activePolicy, label: 'Política de riscos vigente (aprovada em ata)', to: '/policies' },
  { done: progress.activeMandate, label: 'Mandato ativo para operar', to: '/policies' },
]);
const setupComplete = computed(() => checklist.value.every((s) => s.done));

async function loadProgress() {
  const count = (p: Promise<{ totalCount: number }>) => p.then((r) => r.totalCount).catch(() => 0);
  const [policies, mandates] = await Promise.all([
    organization.can(Permission.ViewPolicy) ? api.policies.list({ page: 1, pageSize: 50 }).then((p) => p.items).catch(() => []) : [],
    organization.can(Permission.ViewMandate) ? count(api.mandates.list({ status: 'Active', pageSize: 1 })) : 0,
  ]);
  progress.activePolicy = policies.some((p) => p.status === 'Active');
  progress.activeMandate = mandates > 0;
  progress.loaded = true;
}

// ---------- Dashboards ----------
const storageKey = computed(() => `fix.dashboard.${organization.currentId}`);
const dashboards = ref<DashboardSummary[]>([]);
const current = ref<Dashboard | null>(null);
const loading = ref(true);
const error = ref<string | null>(null);
const editing = ref(false);
const refreshKey = ref(0);

const mine = computed(() => dashboards.value.filter((d) => d.isMine));
const shared = computed(() => dashboards.value.filter((d) => !d.isMine));

async function loadDashboards(selectId?: string) {
  loading.value = true;
  error.value = null;
  try {
    dashboards.value = await api.dashboards.list();
    const preferred = selectId ?? storage.get<string>(storageKey.value);
    const target = dashboards.value.find((d) => d.id === preferred) ?? mine.value[0] ?? dashboards.value[0];
    current.value = target ? await api.dashboards.get(target.id) : null;
  } catch (e) {
    error.value = errorMessage(e);
  } finally {
    loading.value = false;
  }
}

async function select(id: string) {
  editing.value = false;
  storage.set(storageKey.value, id);
  try {
    current.value = await api.dashboards.get(id);
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

function applied(dashboard: Dashboard) {
  current.value = dashboard;
  const summary = dashboards.value.find((d) => d.id === dashboard.id);
  if (summary) Object.assign(summary, { name: dashboard.name, visibility: dashboard.visibility, widgetCount: dashboard.widgets.length });
}

// ---------- Criar / configurar ----------
const dashboardModal = ref<'create' | 'settings' | null>(null);
const dashboardForm = reactive<{ name: string; description: string; visibility: Visibility; template: DashboardTemplate }>({
  name: '',
  description: '',
  visibility: 'Private',
  template: 'fix-overview',
});
const dashboardSubmit = useSubmit();

function openCreate(template: DashboardTemplate = 'fix-overview') {
  Object.assign(dashboardForm, { name: template === 'fix-overview' ? 'Visão geral FIX' : '', description: '', visibility: 'Private', template });
  dashboardSubmit.reset();
  dashboardModal.value = 'create';
}

function openSettings() {
  const d = current.value!;
  Object.assign(dashboardForm, { name: d.name, description: d.description ?? '', visibility: d.visibility });
  dashboardSubmit.reset();
  dashboardModal.value = 'settings';
}

async function saveDashboard() {
  const input = { name: dashboardForm.name, description: dashboardForm.description.trim() || null, visibility: dashboardForm.visibility };
  const creating = dashboardModal.value === 'create';
  const saved = await dashboardSubmit.run(() =>
    creating ? api.dashboards.create({ ...input, template: dashboardForm.template }) : api.dashboards.update(current.value!.id, input),
  );
  if (!saved) return;

  dashboardModal.value = null;
  if (creating) {
    storage.set(storageKey.value, saved.id);
    await loadDashboards(saved.id);
    editing.value = saved.widgets.length === 0;
    toast.success('Dashboard criado.');
  } else {
    applied(saved);
    toast.success('Dashboard atualizado.');
  }
}

async function duplicate() {
  try {
    const copy = await api.dashboards.duplicate(current.value!.id);
    storage.set(storageKey.value, copy.id);
    await loadDashboards(copy.id);
    toast.success('Cópia privada criada: personalize à vontade.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

async function remove() {
  const d = current.value!;
  if (!(await confirm({ title: 'Excluir dashboard', message: `Excluir "${d.name}" e seus ${d.widgets.length} widget(s)?`, confirmLabel: 'Excluir', danger: true }))) return;
  try {
    await api.dashboards.remove(d.id);
    storage.remove(storageKey.value);
    editing.value = false;
    await loadDashboards();
    toast.success('Dashboard excluído.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// ---------- Widgets ----------
const editor = ref<{ widget: Widget | null } | null>(null);

function onWidgetSaved(dashboard: Dashboard) {
  applied(dashboard);
  editor.value = null;
  toast.success('Widget salvo.');
}

async function removeWidget(widget: Widget) {
  if (!(await confirm({ title: 'Remover widget', message: `Remover "${widget.title}"?`, confirmLabel: 'Remover', danger: true }))) return;
  try {
    applied(await api.dashboards.removeWidget(current.value!.id, widget.id));
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

async function reorder(ids: string[]) {
  const d = current.value!;
  const previous = d.widgets;
  current.value = { ...d, widgets: ids.map((id) => previous.find((w) => w.id === id)!) };
  try {
    applied(await api.dashboards.reorderWidgets(d.id, ids));
  } catch (e) {
    current.value = { ...d, widgets: previous };
    toast.error(errorMessage(e));
  }
}

function move(index: number, delta: number) {
  const ids = current.value!.widgets.map((w) => w.id);
  const [id] = ids.splice(index, 1);
  ids.splice(index + delta, 0, id!);
  reorder(ids);
}

async function resize(widget: Widget, width: number) {
  const { id, ...definition } = widget;
  try {
    applied(await api.dashboards.updateWidget(current.value!.id, id, { ...definition, layout: { ...definition.layout, width } }));
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// ---------- Arrastar na grade (modo edição) ----------
// Enquanto o card é segurado, os demais se reorganizam ao vivo (SortableJS com animação); a ordem é salva ao soltar.
const order = ref<Widget[]>([]);
const dragging = ref(false);
watch(
  () => current.value?.widgets,
  (widgets) => {
    if (!dragging.value) order.value = [...(widgets ?? [])];
  },
  { immediate: true },
);

function dropped() {
  dragging.value = false;
  const ids = order.value.map((w) => w.id);
  if (ids.join() !== current.value!.widgets.map((w) => w.id).join()) reorder(ids);
}

onMounted(() => {
  catalog.load().catch((e) => toast.error(errorMessage(e)));
  loadProgress();
  loadDashboards();
});
</script>

<template>
  <PageHeader
    title="Home"
    :subtitle="setup ? `${setup.profile.corporateName} · ${sectorLabel[setup.profile.sector]} · safra ${setup.profile.activeCrop ?? '—'}` : null"
  >
    <template #actions>
      <template v-if="dashboards.length">
        <select class="input" style="width: auto" :value="current?.id" aria-label="Dashboard" @change="select(($event.target as HTMLSelectElement).value)">
          <optgroup v-if="mine.length" label="Meus dashboards">
            <option v-for="d in mine" :key="d.id" :value="d.id">{{ d.name }}{{ d.visibility === 'Public' ? ' · público' : '' }}</option>
          </optgroup>
          <optgroup v-if="shared.length" label="Da organização">
            <option v-for="d in shared" :key="d.id" :value="d.id">{{ d.name }}</option>
          </optgroup>
        </select>
        <button class="btn" type="button" title="Recalcular os widgets" @click="refreshKey++">Atualizar</button>
      </template>
      <button class="btn btn-primary" type="button" @click="openCreate()">+ Dashboard</button>
    </template>
  </PageHeader>

  <section v-if="progress.loaded && !setupComplete" class="card checklist-card">
    <header class="card-header"><h2>Como começar</h2></header>
    <ol class="checklist card-body">
      <li v-for="step in checklist" :key="step.label" :class="{ done: step.done }">
        <span aria-hidden="true">{{ step.done ? '✓' : '○' }}</span>
        <RouterLink :to="step.to">{{ step.label }}</RouterLink>
      </li>
    </ol>
  </section>

  <StateBlock :loading="loading && !current" :error="error" @retry="loadDashboards()">
    <section v-if="!current" class="card card-body empty">
      <h2>Monte o seu painel</h2>
      <p class="muted" style="margin: 0">
        Dashboards reúnem KPIs, gráficos e tabelas sobre mandatos, boletas, contrapartes e políticas. Cada widget tem tipo, tamanho, filtros e cores
        próprios. Você pode mantê-lo privado ou publicá-lo para a organização.
      </p>
      <div class="row">
        <button class="btn btn-primary" type="button" @click="openCreate('fix-overview')">Começar pelo modelo FIX</button>
        <button class="btn" type="button" @click="openCreate('blank')">Dashboard em branco</button>
      </div>
    </section>

    <template v-else>
      <div class="toolbar">
        <div class="stack" style="gap: 2px">
          <div class="row">
            <span class="badge" :class="current.visibility === 'Public' ? 'badge-info' : ''">{{ visibilityLabel[current.visibility] }}</span>
            <span v-if="!current.isMine" class="muted small">compartilhado pela organização{{ current.canEdit ? ' · você pode editar' : '' }}</span>
          </div>
          <p v-if="current.description" class="muted small" style="margin: 0">{{ current.description }}</p>
        </div>
        <div class="row">
          <template v-if="current.canEdit">
            <button v-if="editing" class="btn btn-primary btn-sm" type="button" @click="editor = { widget: null }">+ Widget</button>
            <button class="btn btn-sm" type="button" :aria-pressed="editing" @click="editing = !editing">{{ editing ? 'Concluir edição' : 'Editar layout' }}</button>
            <button class="btn btn-sm" type="button" @click="openSettings">Configurar</button>
          </template>
          <button class="btn btn-sm" type="button" @click="duplicate">Duplicar</button>
          <button v-if="current.canEdit" class="btn btn-sm btn-danger" type="button" @click="remove">Excluir</button>
        </div>
      </div>

      <p v-if="editing" class="alert alert-info small">
        Segure um widget e arraste: os outros abrem espaço enquanto você move, e a nova ordem é salva ao soltar. Use − / + para a largura e ⚙ para
        tipo, dados, altura e cores.
      </p>

      <VueDraggable
        v-model="order"
        class="grid"
        :class="{ editing, dragging }"
        :disabled="!editing"
        :animation="220"
        easing="cubic-bezier(0.2, 0.8, 0.2, 1)"
        :force-fallback="true"
        :fallback-tolerance="4"
        filter="button, select, input, a"
        :prevent-on-filter="false"
        ghost-class="widget-ghost"
        chosen-class="widget-chosen"
        drag-class="widget-drag"
        @start="dragging = true"
        @end="dropped"
      >
        <WidgetCard
          v-for="(w, i) in order"
          :key="w.id"
          :dashboard-id="current.id"
          :widget="w"
          :editing="editing"
          :first="i === 0"
          :last="i === order.length - 1"
          :refresh-key="refreshKey"
          @edit="editor = { widget: w }"
          @remove="removeWidget(w)"
          @move="move(i, $event)"
          @resize="resize(w, $event)"
        />
      </VueDraggable>
      <button v-if="editing && current.widgets.length < 24" class="add-widget" type="button" @click="editor = { widget: null }">+ Adicionar widget</button>
      <p v-if="!current.widgets.length && !editing" class="muted">Este dashboard ainda não tem widgets.</p>
    </template>
  </StateBlock>

  <WidgetEditorModal v-if="editor && current" :dashboard="current" :widget="editor.widget" @close="editor = null" @saved="onWidgetSaved" />

  <BaseModal v-if="dashboardModal" :title="dashboardModal === 'create' ? 'Novo dashboard' : 'Configurar dashboard'" width="480px" @close="dashboardModal = null">
    <form id="dashboard-form" class="stack" @submit.prevent="saveDashboard">
      <p v-if="dashboardSubmit.error.value" class="alert alert-error">{{ dashboardSubmit.error.value }}</p>
      <div class="field">
        <label for="dashboard-name">Nome</label>
        <input id="dashboard-name" v-model="dashboardForm.name" class="input" maxlength="120" required autofocus />
      </div>
      <div class="field">
        <label for="dashboard-description">Descrição</label>
        <textarea id="dashboard-description" v-model="dashboardForm.description" class="input" maxlength="500" />
      </div>
      <div v-if="dashboardModal === 'create'" class="field">
        <label for="dashboard-template">Começar com</label>
        <select id="dashboard-template" v-model="dashboardForm.template" class="input">
          <option value="fix-overview">Modelo FIX (KPIs, evolução, composição e rankings)</option>
          <option value="blank">Em branco</option>
        </select>
      </div>
      <div class="field">
        <label for="dashboard-visibility">Visibilidade</label>
        <select id="dashboard-visibility" v-model="dashboardForm.visibility" class="input" :disabled="dashboardModal === 'settings' && !current?.isMine">
          <option v-for="v in VISIBILITIES" :key="v" :value="v">{{ visibilityLabel[v] }}</option>
        </select>
        <span class="muted small">Público: todos os membros veem; editam o dono e quem gerencia a organização.</span>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="dashboardModal = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="dashboard-form" :disabled="dashboardSubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.checklist-card {
  margin-bottom: 16px;
}

.checklist {
  list-style: none;
  margin: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 8px 24px;
}

.checklist li {
  display: flex;
  gap: 8px;
}

.checklist li.done {
  color: var(--text-muted);
}

.checklist li.done a {
  color: var(--success);
}

.empty {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 720px;
}

.toolbar {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 12px;
}

.grid {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: 16px;
  align-items: start;
}

/* Em edição o card inteiro é a alça de arraste. */
.grid.editing :deep(.widget) {
  cursor: grab;
}

.grid.dragging :deep(.widget) {
  cursor: grabbing;
}

/* Lugar onde o card vai cair: contorno tracejado dourado. */
.grid :deep(.widget-ghost) {
  opacity: 0.35;
  border: 2px dashed var(--primary);
  background: var(--primary-soft);
  box-shadow: none;
}

/* Card "na mão": levemente inclinado e com sombra, seguindo o cursor. */
:global(.widget-drag) {
  opacity: 0.95 !important;
  transform: rotate(1.2deg);
  box-shadow: var(--shadow-lg) !important;
  cursor: grabbing;
}

.add-widget {
  display: block;
  width: 100%;
  margin-top: 16px;
  min-height: 72px;
  border: 1px dashed var(--border);
  border-radius: var(--radius);
  background: none;
  color: var(--text-muted);
  font: inherit;
  cursor: pointer;
}

.add-widget:hover {
  border-color: var(--primary);
  color: var(--primary);
}

/* Tablet: 6 colunas (cards de 1/3 viram metade). Celular: um card por linha. */
@media (max-width: 1100px) {
  .grid {
    grid-template-columns: repeat(6, minmax(0, 1fr));
  }

  .grid :deep(.widget) {
    grid-column: span 6 !important;
  }

  .grid :deep(.widget.span-3),
  .grid :deep(.widget.span-4) {
    grid-column: span 3 !important;
  }
}

@media (max-width: 720px) {
  .grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .grid :deep(.widget),
  .grid :deep(.widget.span-3),
  .grid :deep(.widget.span-4) {
    grid-column: 1 / -1 !important;
  }
}
</style>
