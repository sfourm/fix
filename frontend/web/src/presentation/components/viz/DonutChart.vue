<script setup lang="ts">
import { computed, ref } from 'vue';
import type { WidgetData } from '@/domain/dashboard';
import ChartTooltip from './ChartTooltip.vue';
import { formatValue, useElementSize } from './viz';

/** Rosca: composição de um todo em poucas partes (≤ 8), com legenda sempre visível (cor nunca sozinha). */
const props = defineProps<{ data: WidgetData; color: (index: number, key?: string) => string }>();

const root = ref<HTMLElement | null>(null);
const { width, height } = useElementSize(root);
const hover = ref<number | null>(null);

const layout = computed(() => {
  const points = props.data.points.filter((p) => (p.value ?? 0) > 0);
  const total = points.reduce((sum, p) => sum + (p.value ?? 0), 0);
  const legendBeside = width.value >= 320;
  const size = legendBeside ? Math.min(height.value, width.value * 0.5) : Math.min(width.value, height.value * 0.6);
  const r = Math.max(size / 2 - 4, 10);
  const inner = r * 0.62;
  const cx = legendBeside ? r + 4 : width.value / 2;
  const cy = legendBeside ? height.value / 2 : r + 4;

  let angle = -Math.PI / 2;
  const slices = points.map((p) => {
    const share = total ? (p.value ?? 0) / total : 0;
    const start = angle;
    const end = angle + share * Math.PI * 2;
    angle = end;
    const mid = (start + end) / 2;
    return { point: p, share, path: arc(cx, cy, r, inner, start, end), tx: cx + Math.cos(mid) * r, ty: cy + Math.sin(mid) * r };
  });

  return { slices, total, cx, cy, legendBeside, index: (key: string) => props.data.points.findIndex((p) => p.key === key) };
});

function arc(cx: number, cy: number, r: number, inner: number, start: number, end: number): string {
  if (end - start >= Math.PI * 2 - 1e-6) {
    // Fatia única: dois semicírculos (um arco SVG não fecha 360°).
    return `M${cx - r},${cy}a${r},${r} 0 1,0 ${2 * r},0a${r},${r} 0 1,0 ${-2 * r},0Z M${cx - inner},${cy}a${inner},${inner} 0 1,1 ${2 * inner},0a${inner},${inner} 0 1,1 ${-2 * inner},0Z`;
  }

  const large = end - start > Math.PI ? 1 : 0;
  const p = (radius: number, a: number) => `${cx + Math.cos(a) * radius},${cy + Math.sin(a) * radius}`;
  return `M${p(r, start)}A${r},${r} 0 ${large} 1 ${p(r, end)}L${p(inner, end)}A${inner},${inner} 0 ${large} 0 ${p(inner, start)}Z`;
}

const pct = (share: number) => `${(share * 100).toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;
</script>

<template>
  <div ref="root" class="chart donut" :class="{ stacked: !layout.legendBeside }" @mouseleave="hover = null">
    <svg v-if="width > 0" :width="width" :height="height" role="img" :aria-label="`Composição de ${data.measureLabel} por ${data.groupLabel}`">
      <!-- Traço da cor da superfície = 2px de separação entre as fatias -->
      <path
        v-for="(s, i) in layout.slices"
        :key="s.point.key"
        :d="s.path"
        :fill="color(layout.index(s.point.key), s.point.key)"
        stroke="var(--surface)"
        stroke-width="2"
        fill-rule="evenodd"
        :opacity="hover === null || hover === i ? 1 : 0.55"
        @mouseenter="hover = i"
      />
      <text class="total" :x="layout.cx" :y="layout.cy - 4" text-anchor="middle">{{ formatValue(layout.total, data.format, true) }}</text>
      <text class="axis" :x="layout.cx" :y="layout.cy + 14" text-anchor="middle">total</text>
    </svg>
    <ul class="legend" :class="{ beside: layout.legendBeside }">
      <li v-for="(s, i) in layout.slices" :key="s.point.key" :class="{ dim: hover !== null && hover !== i }" @mouseenter="hover = i">
        <span class="swatch" :style="{ background: color(layout.index(s.point.key), s.point.key) }" />
        <span class="name">{{ s.point.label }}</span>
        <span class="num">{{ formatValue(s.point.value, data.format, true) }} · {{ pct(s.share) }}</span>
      </li>
    </ul>
    <ChartTooltip
      v-if="hover !== null && layout.slices[hover]"
      :x="layout.slices[hover]!.tx"
      :y="layout.slices[hover]!.ty"
      :width="width"
      :title="layout.slices[hover]!.point.label"
      :value="`${formatValue(layout.slices[hover]!.point.value, data.format)} · ${pct(layout.slices[hover]!.share)}`"
      :detail="`${layout.slices[hover]!.point.count} registro(s)`"
      :color="color(layout.index(layout.slices[hover]!.point.key), layout.slices[hover]!.point.key)"
    />
  </div>
</template>

<style scoped>
@import './chart.css';

.donut svg {
  position: absolute;
  inset: 0;
}

.total {
  fill: var(--text);
  font-size: 18px;
  font-weight: 700;
}

.legend {
  position: absolute;
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 0.8rem;
}

.legend.beside {
  top: 50%;
  right: 0;
  width: 46%;
  transform: translateY(-50%);
}

.stacked .legend {
  left: 0;
  right: 0;
  bottom: 0;
  flex-direction: row;
  flex-wrap: wrap;
  gap: 4px 12px;
}

.legend li {
  display: grid;
  grid-template-columns: 10px minmax(0, 1fr) auto;
  align-items: center;
  gap: 6px;
}

.legend li.dim {
  opacity: 0.55;
}

.swatch {
  width: 10px;
  height: 10px;
  border-radius: 3px;
}

.name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.num {
  color: var(--text-muted);
  font-variant-numeric: tabular-nums;
}
</style>
