<script setup lang="ts">
import { computed } from 'vue';
import type { WidgetData, WidgetDefinition } from '@/domain/dashboard';
import BarChart from './BarChart.vue';
import ColumnChart from './ColumnChart.vue';
import DataTable from './DataTable.vue';
import DonutChart from './DonutChart.vue';
import LineChart from './LineChart.vue';
import { colorResolver, formatValue } from './viz';

/** Renderiza os dados no formato escolhido para o widget (ou em tabela, para leitura acessível). */
const props = defineProps<{ widget: Pick<WidgetDefinition, 'type' | 'colors'>; data: WidgetData; asTable?: boolean }>();

const color = computed(() => colorResolver(props.widget.type === 'Donut' ? 'category' : props.widget.colors.mode, props.widget.colors.palette));
const empty = computed(() => props.widget.type !== 'Kpi' && props.data.points.length === 0);
</script>

<template>
  <div v-if="widget.type === 'Kpi'" class="kpi">
    <strong :style="widget.colors.palette[0] ? { color: widget.colors.palette[0] } : undefined">{{ formatValue(data.total, data.format) }}</strong>
    <span class="muted small">{{ data.measureLabel }} · {{ data.rowCount }} registro(s)</span>
  </div>
  <div v-else-if="empty" class="empty muted small">Sem dados para os filtros deste widget.</div>
  <DataTable v-else-if="asTable || widget.type === 'Table'" :data="data" :color="widget.type === 'Table' && widget.colors.mode === 'single' ? undefined : color" />
  <ColumnChart v-else-if="widget.type === 'Column'" :data="data" :color="color" />
  <BarChart v-else-if="widget.type === 'Bar'" :data="data" :color="color" />
  <LineChart v-else-if="widget.type === 'Line'" :data="data" :color="color" />
  <DonutChart v-else :data="data" :color="color" />
</template>

<style scoped>
.kpi {
  display: flex;
  flex-direction: column;
  justify-content: center;
  height: 100%;
  gap: 4px;
}

.kpi strong {
  font-size: clamp(1.6rem, 3vw, 2.4rem);
  line-height: 1.1;
  font-variant-numeric: tabular-nums;
}

.empty {
  display: grid;
  place-items: center;
  height: 100%;
  text-align: center;
}
</style>
