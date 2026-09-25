<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import { useCatalogStore } from '@/application/stores/catalog.store';
import {
  AGGREGATIONS,
  aggregationLabel,
  GRID_COLUMNS,
  MAX_WIDGET_HEIGHT,
  MIN_WIDGET_HEIGHT,
  WIDGET_TYPES,
  widgetTypeLabel,
  type Dashboard,
  type Widget,
  type WidgetData,
  type WidgetDefinition,
  type WidgetType,
} from '@/domain/dashboard';
import type { DataSource, SavedFilter } from '@/domain/search';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import BaseModal from '../BaseModal.vue';
import FilterBuilder from '../filters/FilterBuilder.vue';
import WidgetChart from '../viz/WidgetChart.vue';
import { DEFAULT_PALETTE, PALETTE_PRESETS, SERIES_SLOTS } from '../viz/viz';

/** Editor de widget: tipo, dados (conjunto, medida, dimensão, filtros), tamanho e cores, com prévia ao vivo. */
const props = defineProps<{ dashboard: Dashboard; widget: Widget | null }>();
const emit = defineEmits<{ close: []; saved: [dashboard: Dashboard] }>();

const api = useApi();
const catalog = useCatalogStore();
const sources = computed(() => catalog.readable());

const initial = (): WidgetDefinition => {
  if (props.widget) {
    const { id: _, ...definition } = JSON.parse(JSON.stringify(props.widget)) as Widget;
    return definition;
  }

  const source = sources.value[0]?.key ?? 'orders';
  return {
    title: '',
    type: 'Column',
    layout: { width: 6, height: 300 },
    query: { source, aggregation: 'count', measureField: null, groupBy: firstGroupable(source), filterId: null, criteria: [], sort: 'value-desc', limit: 8 },
    colors: { mode: 'single', palette: [] },
  };
};

function firstGroupable(source: DataSource): string | null {
  return catalog.source(source)?.fields.find((f) => f.groupable && f.type === 'enum')?.key ?? catalog.source(source)?.fields.find((f) => f.groupable)?.key ?? null;
}

const form = reactive<WidgetDefinition>(initial());
const fields = computed(() => catalog.source(form.query.source)?.fields ?? []);
const measurable = computed(() => fields.value.filter((f) => f.measurable));
const groupable = computed(() => fields.value.filter((f) => f.groupable));

// ---------- Coerência entre tipo, dados e cores ----------
function changeType(type: WidgetType) {
  form.type = type;
  if (type === 'Kpi') {
    form.query.groupBy = null;
    form.layout = { width: Math.min(form.layout.width, 4), height: Math.min(form.layout.height, 160) };
  } else {
    form.query.groupBy ??= firstGroupable(form.query.source);
    if (form.layout.height < 220) form.layout.height = 300;
  }
  if (type === 'Line') {
    form.query.sort = 'label';
    const month = fields.value.find((f) => f.format === 'month');
    if (month) form.query.groupBy = month.key;
  }
  if (type === 'Donut') {
    form.query.limit = Math.min(form.query.limit, 6);
    form.colors.mode = 'category';
  }
}

function changeSource(source: DataSource) {
  form.query = { ...form.query, source, measureField: null, aggregation: 'count', filterId: null, criteria: [], groupBy: form.type === 'Kpi' ? null : firstGroupable(source) };
  loadFilters();
}

watch(
  () => form.query.aggregation,
  (aggregation) => {
    form.query.measureField = aggregation === 'count' ? null : (form.query.measureField ?? measurable.value[0]?.key ?? null);
  },
);

// ---------- Filtros salvos (dashboard público só usa filtro público) ----------
const filters = ref<SavedFilter[]>([]);
const usableFilters = computed(() => filters.value.filter((f) => props.dashboard.visibility === 'Private' || f.visibility === 'Public'));
async function loadFilters() {
  filters.value = await api.filters.list(form.query.source).catch(() => []);
}

// ---------- Cores ----------
const colorSlots = computed(() => (form.colors.mode === 'single' || form.type === 'Kpi' ? 1 : Math.min(Math.max(preview.value?.points.length ?? 4, 1), SERIES_SLOTS + 4)));
function applyPreset(palette: string[]) {
  form.colors.palette = [...palette];
}
function setColor(index: number, color: string) {
  const palette = form.colors.palette.length ? [...form.colors.palette] : DEFAULT_PALETTE.slice(0, Math.max(colorSlots.value, 1));
  while (palette.length <= index) palette.push(DEFAULT_PALETTE[palette.length % DEFAULT_PALETTE.length]!);
  palette[index] = color;
  form.colors.palette = palette;
}
const shownColor = (index: number) => form.colors.palette[index % Math.max(form.colors.palette.length, 1)] ?? DEFAULT_PALETTE[index % DEFAULT_PALETTE.length]!;

// ---------- Prévia ----------
const preview = ref<WidgetData | null>(null);
const previewError = ref<string | null>(null);
let timer: ReturnType<typeof setTimeout> | undefined;
watch(
  () => JSON.stringify([form.type, form.query]),
  () => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      try {
        preview.value = await api.search.preview({ type: form.type, query: form.query });
        previewError.value = null;
      } catch (e) {
        previewError.value = errorMessage(e);
      }
    }, 350);
  },
  { immediate: true },
);
onBeforeUnmount(() => clearTimeout(timer));

// ---------- Salvar ----------
const submit = useSubmit();
async function save() {
  const definition: WidgetDefinition = JSON.parse(JSON.stringify(form));
  const saved = await submit.run(() =>
    props.widget ? api.dashboards.updateWidget(props.dashboard.id, props.widget.id, definition) : api.dashboards.addWidget(props.dashboard.id, definition),
  );
  if (saved) emit('saved', saved);
}

onMounted(loadFilters);
</script>

<template>
  <BaseModal :title="widget ? 'Configurar widget' : 'Novo widget'" width="1080px" @close="emit('close')">
    <div class="editor">
      <form id="widget-form" class="stack" @submit.prevent="save">
        <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>

        <div class="field">
          <label for="widget-title">Título</label>
          <input id="widget-title" v-model="form.title" class="input" maxlength="80" required placeholder="Ex.: Lotes por contraparte" />
        </div>

        <div class="field">
          <label>Tipo de visualização</label>
          <div class="types" role="radiogroup">
            <button
              v-for="t in WIDGET_TYPES"
              :key="t"
              type="button"
              class="type"
              role="radio"
              :aria-checked="form.type === t"
              :class="{ active: form.type === t }"
              @click="changeType(t)"
            >
              {{ widgetTypeLabel[t] }}
            </button>
          </div>
        </div>

        <h3 class="section-title">Dados</h3>
        <div class="form-grid">
          <div class="field">
            <label for="widget-source">Conjunto de dados</label>
            <select id="widget-source" class="input" :value="form.query.source" @change="changeSource(($event.target as HTMLSelectElement).value as DataSource)">
              <option v-for="s in sources" :key="s.key" :value="s.key">{{ s.label }}</option>
            </select>
          </div>
          <div class="field">
            <label for="widget-aggregation">Medida</label>
            <div class="row" style="flex-wrap: nowrap">
              <select id="widget-aggregation" v-model="form.query.aggregation" class="input">
                <option v-for="a in AGGREGATIONS" :key="a" :value="a" :disabled="a !== 'count' && measurable.length === 0">{{ aggregationLabel[a] }}</option>
              </select>
              <select v-if="form.query.aggregation !== 'count'" v-model="form.query.measureField" class="input" aria-label="Campo medido">
                <option v-for="f in measurable" :key="f.key" :value="f.key">{{ f.label }}</option>
              </select>
            </div>
          </div>
          <div v-if="form.type !== 'Kpi'" class="field">
            <label for="widget-group">Agrupar por</label>
            <select id="widget-group" v-model="form.query.groupBy" class="input">
              <option v-for="f in groupable" :key="f.key" :value="f.key">{{ f.label }}</option>
            </select>
          </div>
          <div v-if="form.type !== 'Kpi'" class="field">
            <label for="widget-sort">Ordem e limite</label>
            <div class="row" style="flex-wrap: nowrap">
              <select id="widget-sort" v-model="form.query.sort" class="input">
                <option value="value-desc">Maior valor primeiro</option>
                <option value="value-asc">Menor valor primeiro</option>
                <option value="label">Pela categoria (tempo)</option>
              </select>
              <input v-model.number="form.query.limit" class="input" type="number" min="1" :max="form.type === 'Donut' ? 8 : 24" style="width: 80px" aria-label="Máximo de categorias" />
            </div>
          </div>
        </div>

        <div class="field">
          <label for="widget-filter">Filtro salvo</label>
          <select id="widget-filter" v-model="form.query.filterId" class="input">
            <option :value="null">Nenhum</option>
            <option v-for="f in usableFilters" :key="f.id" :value="f.id">{{ f.name }}{{ f.visibility === 'Public' ? ' · público' : '' }}</option>
          </select>
          <span v-if="dashboard.visibility === 'Public'" class="muted small">Dashboard público: só filtros públicos, para que toda a organização veja os mesmos dados.</span>
        </div>
        <div class="field">
          <label>Condições do widget</label>
          <FilterBuilder v-model="form.query.criteria" :source="form.query.source" />
        </div>

        <h3 class="section-title">Tamanho</h3>
        <div class="form-grid">
          <div class="field">
            <label for="widget-width">Largura: {{ form.layout.width }}/{{ GRID_COLUMNS }} colunas</label>
            <input id="widget-width" v-model.number="form.layout.width" type="range" min="2" :max="GRID_COLUMNS" step="1" />
          </div>
          <div class="field">
            <label for="widget-height">Altura: {{ form.layout.height }} px</label>
            <input id="widget-height" v-model.number="form.layout.height" type="range" :min="MIN_WIDGET_HEIGHT" :max="MAX_WIDGET_HEIGHT" step="10" />
          </div>
        </div>

        <h3 class="section-title">Cores</h3>
        <div class="row">
          <select v-if="form.type !== 'Kpi' && form.type !== 'Donut'" v-model="form.colors.mode" class="input input-sm" aria-label="Modo de cor">
            <option value="single">Uma cor para todas as barras</option>
            <option value="category">Uma cor por categoria</option>
          </select>
          <select class="input input-sm" aria-label="Paleta pronta" @change="applyPreset(PALETTE_PRESETS[Number(($event.target as HTMLSelectElement).value)]!.palette)">
            <option value="" disabled selected>Paletas prontas…</option>
            <option v-for="(p, i) in PALETTE_PRESETS" :key="p.name" :value="i">{{ p.name }}</option>
          </select>
        </div>
        <div class="swatches">
          <label v-for="i in colorSlots" :key="i" class="swatch-input" :title="preview?.points[i - 1]?.label ?? `Cor ${i}`">
            <input type="color" :value="shownColor(i - 1)" :aria-label="`Cor ${i}`" @input="setColor(i - 1, ($event.target as HTMLInputElement).value)" />
            <span class="small truncate">{{ form.colors.mode === 'single' || form.type === 'Kpi' ? 'Cor' : (preview?.points[i - 1]?.label ?? `Cor ${i}`) }}</span>
          </label>
        </div>
        <p class="muted small" style="margin: 0">
          {{ form.colors.palette.length ? 'Cores personalizadas.' : 'Paleta padrão do tema (ajustada para daltonismo e modo escuro).' }}
          <button v-if="form.colors.palette.length" class="btn-link btn small" type="button" @click="applyPreset([])">Restaurar padrão</button>
        </p>
      </form>

      <aside class="preview">
        <span class="muted small">Prévia</span>
        <div class="preview-box card" :style="{ height: `${Math.min(form.layout.height, 420)}px`, width: `${Math.max((form.layout.width / GRID_COLUMNS) * 100, 40)}%` }">
          <strong class="small">{{ form.title || 'Sem título' }}</strong>
          <div class="preview-chart">
            <p v-if="previewError" class="alert alert-error small">{{ previewError }}</p>
            <WidgetChart v-else-if="preview" :widget="form" :data="preview" />
          </div>
        </div>
        <span v-if="preview" class="muted small">{{ preview.rowCount }} registro(s){{ preview.cached ? ' · do cache' : '' }}</span>
      </aside>
    </div>
    <template #footer>
      <button class="btn" type="button" @click="emit('close')">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="widget-form" :disabled="submit.submitting.value">Salvar widget</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.editor {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  gap: 20px;
}

.types {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.type {
  padding: 6px 10px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
  font: inherit;
  font-size: 0.85rem;
  cursor: pointer;
}

.type.active {
  border-color: var(--primary);
  background: var(--primary-soft);
  color: var(--primary);
  font-weight: 600;
}

.swatches {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 14px;
}

.swatch-input {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  max-width: 160px;
}

.swatch-input input {
  width: 32px;
  height: 26px;
  padding: 0;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: none;
  cursor: pointer;
}

.truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.preview {
  position: sticky;
  top: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-self: start;
}

.preview-box {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 240px;
  max-width: 100%;
  padding: 12px 16px;
}

.preview-chart {
  flex: 1;
  min-height: 0;
}

@media (max-width: 900px) {
  .editor {
    grid-template-columns: 1fr;
  }
}
</style>
