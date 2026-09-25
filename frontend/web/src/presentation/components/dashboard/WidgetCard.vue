<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import { GRID_COLUMNS, type Widget, type WidgetData } from '@/domain/dashboard';
import { ApiError, errorMessage } from '@/infrastructure/http/api-error';
import WidgetChart from '../viz/WidgetChart.vue';

/** Widget na grade: busca os próprios dados (com as roles de quem vê) e, em edição, oferece mover/redimensionar. */
const props = defineProps<{ dashboardId: string; widget: Widget; editing: boolean; first: boolean; last: boolean; refreshKey: number }>();
const emit = defineEmits<{ edit: []; remove: []; move: [delta: number]; resize: [width: number] }>();

const api = useApi();
const data = ref<WidgetData | null>(null);
const error = ref<string | null>(null);
const loading = ref(false);
const asTable = ref(false);

async function load() {
  loading.value = true;
  try {
    data.value = await api.dashboards.widgetData(props.dashboardId, props.widget.id);
    error.value = null;
  } catch (e) {
    data.value = null;
    error.value = e instanceof ApiError && e.status === 403 ? 'Você não tem acesso aos dados deste widget.' : errorMessage(e);
  } finally {
    loading.value = false;
  }
}

onMounted(load);
watch(() => [props.widget, props.refreshKey], load, { deep: true });
</script>

<template>
  <article
    class="widget card"
    :class="{ editing }"
    :style="{ gridColumn: `span ${widget.layout.width}`, height: `${widget.layout.height}px` }"
    :aria-label="widget.title"
  >
    <header class="widget-header">
      <h3 :title="widget.title">{{ widget.title }}</h3>
      <div class="row" style="gap: 2px; flex-wrap: nowrap">
        <span v-if="loading" class="spinner" aria-label="Carregando" />
        <template v-if="editing">
          <button class="icon" type="button" :disabled="first" aria-label="Mover para trás" title="Mover para trás" @click="emit('move', -1)">←</button>
          <button class="icon" type="button" :disabled="last" aria-label="Mover para frente" title="Mover para frente" @click="emit('move', 1)">→</button>
          <button class="icon" type="button" :disabled="widget.layout.width <= 2" aria-label="Estreitar" title="Estreitar" @click="emit('resize', widget.layout.width - 1)">−</button>
          <button class="icon" type="button" :disabled="widget.layout.width >= GRID_COLUMNS" aria-label="Alargar" title="Alargar" @click="emit('resize', widget.layout.width + 1)">+</button>
          <button class="icon" type="button" aria-label="Configurar widget" title="Configurar" @click="emit('edit')">⚙</button>
          <button class="icon danger" type="button" aria-label="Remover widget" title="Remover" @click="emit('remove')">✕</button>
        </template>
        <button
          v-else-if="widget.type !== 'Kpi' && widget.type !== 'Table'"
          class="icon"
          type="button"
          :aria-pressed="asTable"
          :title="asTable ? 'Ver gráfico' : 'Ver como tabela'"
          @click="asTable = !asTable"
        >
          {{ asTable ? '◔' : '☰' }}
        </button>
      </div>
    </header>
    <div class="widget-body">
      <p v-if="error" class="muted small">{{ error }}</p>
      <WidgetChart v-else-if="data" :widget="widget" :data="data" :as-table="asTable" />
    </div>
    <footer v-if="data?.truncated" class="muted small">Amostra limitada aos primeiros registros.</footer>
  </article>
</template>

<style scoped>
.widget {
  display: flex;
  flex-direction: column;
  min-width: 0;
  padding: 12px 16px;
  overflow: hidden;
}

.widget.editing {
  outline: 1px dashed var(--primary);
  outline-offset: -1px;
  cursor: grab;
}

.widget-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 8px;
}

.widget-header h3 {
  overflow: hidden;
  font-size: 0.9rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.widget-body {
  flex: 1;
  min-height: 0;
}

.icon {
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;
  border: 0;
  border-radius: var(--radius-sm);
  background: none;
  color: var(--text-muted);
  font: inherit;
  cursor: pointer;
}

.icon:hover:not(:disabled) {
  background: var(--surface-2);
  color: var(--text);
}

.icon:disabled {
  opacity: 0.35;
  cursor: default;
}

.icon.danger:hover {
  color: var(--danger);
}

@media (max-width: 800px) {
  .widget {
    grid-column: 1 / -1 !important;
  }
}
</style>
