<script setup lang="ts">
import { computed, ref } from 'vue';
import type { WidgetData } from '@/domain/dashboard';
import ChartTooltip from './ChartTooltip.vue';
import { formatValue, niceTicks, truncate, useElementSize } from './viz';

/** Linha: evolução de uma medida em categorias ordenadas (meses). Crosshair + dica no ponto mais próximo. */
const props = defineProps<{ data: WidgetData; color: (index: number, key?: string) => string }>();

const root = ref<HTMLElement | null>(null);
const { width, height } = useElementSize(root);
const hover = ref<number | null>(null);

const margin = { top: 16, right: 16, bottom: 34, left: 48 };
const layout = computed(() => {
  const points = props.data.points;
  const values = points.map((p) => p.value ?? 0);
  const ticks = niceTicks(Math.max(...values, 0), Math.min(...values, 0));
  const min = ticks[0]!;
  const max = ticks.at(-1)!;
  const innerW = Math.max(width.value - margin.left - margin.right, 10);
  const innerH = Math.max(height.value - margin.top - margin.bottom, 10);
  const step = points.length > 1 ? innerW / (points.length - 1) : 0;
  const x = (i: number) => margin.left + (points.length > 1 ? step * i : innerW / 2);
  const y = (v: number) => margin.top + innerH - ((v - min) / (max - min || 1)) * innerH;
  const coords = points.map((p, i) => ({ point: p, x: x(i), y: y(p.value ?? 0) }));
  const every = Math.max(Math.ceil(points.length / Math.max(Math.floor(innerW / 64), 1)), 1);

  return {
    ticks: ticks.map((t) => ({ value: t, y: y(t) })),
    coords,
    path: coords.map((c, i) => `${i === 0 ? 'M' : 'L'}${c.x},${c.y}`).join(''),
    labels: coords.filter((_, i) => i % every === 0 || i === coords.length - 1),
    last: coords.at(-1),
    innerBottom: margin.top + innerH,
  };
});

function onMove(event: MouseEvent) {
  const coords = layout.value.coords;
  if (coords.length === 0) return;
  const rect = (event.currentTarget as SVGElement).getBoundingClientRect();
  const mx = event.clientX - rect.left;
  let best = 0;
  coords.forEach((c, i) => {
    if (Math.abs(c.x - mx) < Math.abs(coords[best]!.x - mx)) best = i;
  });
  hover.value = best;
}
</script>

<template>
  <div ref="root" class="chart" @mouseleave="hover = null">
    <svg v-if="width > 0" :width="width" :height="height" role="img" :aria-label="`${data.measureLabel} por ${data.groupLabel}`" @mousemove="onMove">
      <g class="grid">
        <g v-for="t in layout.ticks" :key="t.value">
          <line :x1="margin.left" :x2="width - margin.right" :y1="t.y" :y2="t.y" />
          <text :x="margin.left - 6" :y="t.y" dy="0.32em" text-anchor="end">{{ formatValue(t.value, data.format, true) }}</text>
        </g>
      </g>
      <text v-for="c in layout.labels" :key="c.point.key" class="axis" :x="c.x" :y="height - margin.bottom + 16" text-anchor="middle">
        {{ truncate(c.point.label, 12) }}
      </text>
      <line v-if="hover !== null" class="crosshair" :x1="layout.coords[hover]!.x" :x2="layout.coords[hover]!.x" :y1="margin.top" :y2="layout.innerBottom" />
      <path :d="layout.path" fill="none" :stroke="color(0)" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" />
      <!-- Marcador só no último ponto e no ponto em foco (rótulos seletivos) -->
      <circle v-if="layout.last" :cx="layout.last.x" :cy="layout.last.y" r="4" :fill="color(0)" stroke="var(--surface)" stroke-width="2" />
      <text v-if="layout.last && hover === null" class="value" :x="layout.last.x" :y="layout.last.y - 10" text-anchor="end">
        {{ formatValue(layout.last.point.value, data.format, true) }}
      </text>
      <circle v-if="hover !== null" :cx="layout.coords[hover]!.x" :cy="layout.coords[hover]!.y" r="5" :fill="color(0)" stroke="var(--surface)" stroke-width="2" />
    </svg>
    <ChartTooltip
      v-if="hover !== null && layout.coords[hover]"
      :x="layout.coords[hover]!.x"
      :y="layout.coords[hover]!.y"
      :width="width"
      :title="layout.coords[hover]!.point.label"
      :value="formatValue(layout.coords[hover]!.point.value, data.format)"
      :detail="`${layout.coords[hover]!.point.count} registro(s)`"
      :color="color(0)"
    />
  </div>
</template>

<style scoped>
@import './chart.css';

.crosshair {
  stroke: var(--chart-axis);
  stroke-dasharray: 3 3;
}
</style>
