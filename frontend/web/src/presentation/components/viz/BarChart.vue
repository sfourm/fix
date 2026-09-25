<script setup lang="ts">
import { computed, ref } from 'vue';
import type { WidgetData } from '@/domain/dashboard';
import ChartTooltip from './ChartTooltip.vue';
import { barPath, formatValue, truncate, useElementSize } from './viz';

/** Barras horizontais: rankings e categorias com rótulos longos (contrapartes, mandatos). */
const props = defineProps<{ data: WidgetData; color: (index: number, key?: string) => string }>();

const root = ref<HTMLElement | null>(null);
const { width, height } = useElementSize(root);
const hover = ref<number | null>(null);

const layout = computed(() => {
  const points = props.data.points;
  const labelW = Math.min(Math.max(width.value * 0.32, 80), 180);
  const valueW = 64;
  const innerW = Math.max(width.value - labelW - valueW - 8, 10);
  const band = Math.max(height.value / Math.max(points.length, 1), 1);
  const barH = Math.max(Math.min(band - 6, 22), 2); // ≥2px de superfície entre barras
  const max = Math.max(...points.map((p) => Math.abs(p.value ?? 0)), 1e-9);

  return {
    labelW,
    labelMax: Math.max(Math.floor(labelW / 6.8), 4),
    bars: points.map((p, i) => {
      const w = (Math.abs(p.value ?? 0) / max) * innerW;
      const y = band * i + (band - barH) / 2;
      return { point: p, y, w, barH, cy: y + barH / 2, path: barPath(labelW, y, w, barH, 'right') };
    }),
  };
});
</script>

<template>
  <div ref="root" class="chart" @mouseleave="hover = null">
    <svg v-if="width > 0" :width="width" :height="height" role="img" :aria-label="`${data.measureLabel} por ${data.groupLabel}`">
      <g v-for="(b, i) in layout.bars" :key="b.point.key">
        <rect x="0" :y="b.cy - (b.barH + 6) / 2" :width="width" :height="b.barH + 6" fill="transparent" @mouseenter="hover = i" />
        <text class="axis" :x="layout.labelW - 8" :y="b.cy" dy="0.32em" text-anchor="end">{{ truncate(b.point.label, layout.labelMax) }}</text>
        <path :d="b.path" :fill="color(i, b.point.key)" :opacity="hover === null || hover === i ? 1 : 0.55" pointer-events="none" />
        <text class="value" :x="layout.labelW + b.w + 6" :y="b.cy" dy="0.32em">{{ formatValue(b.point.value, data.format, true) }}</text>
      </g>
      <line class="baseline" :x1="layout.labelW" :x2="layout.labelW" y1="0" :y2="height" />
    </svg>
    <ChartTooltip
      v-if="hover !== null && layout.bars[hover]"
      :x="layout.labelW + layout.bars[hover]!.w / 2"
      :y="layout.bars[hover]!.y"
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
