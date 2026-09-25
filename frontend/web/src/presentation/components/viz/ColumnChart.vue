<script setup lang="ts">
import { computed, ref } from 'vue';
import type { WidgetData } from '@/domain/dashboard';
import ChartTooltip from './ChartTooltip.vue';
import { barPath, formatValue, niceTicks, truncate, useElementSize } from './viz';

/** Colunas verticais: comparar magnitudes entre poucas categorias (rótulos curtos). */
const props = defineProps<{ data: WidgetData; color: (index: number, key?: string) => string }>();

const root = ref<HTMLElement | null>(null);
const { width, height } = useElementSize(root);
const hover = ref<number | null>(null);

const margin = { top: 18, right: 8, bottom: 34, left: 48 };
const layout = computed(() => {
  const points = props.data.points;
  const values = points.map((p) => p.value ?? 0);
  const ticks = niceTicks(Math.max(...values, 0), Math.min(...values, 0));
  const min = ticks[0]!;
  const max = ticks.at(-1)!;
  const innerW = Math.max(width.value - margin.left - margin.right, 10);
  const innerH = Math.max(height.value - margin.top - margin.bottom, 10);
  const band = innerW / Math.max(points.length, 1);
  const barW = Math.max(Math.min(band * 0.72, 56), 2);
  const y = (v: number) => margin.top + innerH - ((v - min) / (max - min || 1)) * innerH;

  return {
    ticks: ticks.map((t) => ({ value: t, y: y(t) })),
    zero: y(0),
    labelMax: Math.max(Math.floor(band / 6.5), 3),
    showValues: points.length <= 12 && band >= 28,
    bars: points.map((p, i) => {
      const v = p.value ?? 0;
      const x = margin.left + band * i + (band - barW) / 2;
      const top = y(Math.max(v, 0));
      const h = Math.abs(y(v) - y(0));
      return { point: p, x, w: barW, top, h, cx: x + barW / 2, path: v >= 0 ? barPath(x, top, barW, h, 'up') : `M${x},${y(0)}h${barW}v${h}h${-barW}Z` };
    }),
  };
});
</script>

<template>
  <div ref="root" class="chart" @mouseleave="hover = null">
    <svg v-if="width > 0" :width="width" :height="height" role="img" :aria-label="`${data.measureLabel} por ${data.groupLabel}`">
      <g class="grid">
        <g v-for="t in layout.ticks" :key="t.value">
          <line :x1="margin.left" :x2="width - margin.right" :y1="t.y" :y2="t.y" />
          <text :x="margin.left - 6" :y="t.y" dy="0.32em" text-anchor="end">{{ formatValue(t.value, data.format, true) }}</text>
        </g>
      </g>
      <g v-for="(b, i) in layout.bars" :key="b.point.key">
        <!-- Área de acerto maior que a marca -->
        <rect :x="b.cx - (b.w / 0.72) / 2" :y="margin.top" :width="b.w / 0.72" :height="height - margin.top - margin.bottom" fill="transparent" @mouseenter="hover = i" />
        <path :d="b.path" :fill="color(i, b.point.key)" :opacity="hover === null || hover === i ? 1 : 0.55" pointer-events="none" />
        <text v-if="layout.showValues" class="value" :x="b.cx" :y="b.top - 5" text-anchor="middle">{{ formatValue(b.point.value, data.format, true) }}</text>
        <text class="axis" :x="b.cx" :y="height - margin.bottom + 16" text-anchor="middle">{{ truncate(b.point.label, layout.labelMax) }}</text>
      </g>
      <line class="baseline" :x1="margin.left" :x2="width - margin.right" :y1="layout.zero" :y2="layout.zero" />
    </svg>
    <ChartTooltip
      v-if="hover !== null && layout.bars[hover]"
      :x="layout.bars[hover]!.cx"
      :y="layout.bars[hover]!.top"
      :width="width"
      :title="layout.bars[hover]!.point.label"
      :value="formatValue(layout.bars[hover]!.point.value, data.format)"
      :detail="`${layout.bars[hover]!.point.count} registro(s)`"
      :color="color(hover, layout.bars[hover]!.point.key)"
    />
  </div>
</template>

<style scoped>
@import './chart.css';
</style>
