<script setup lang="ts">
import { computed } from 'vue';
import type { WidgetData } from '@/domain/dashboard';
import { formatValue } from './viz';

/** Tabela dos pontos: tipo de widget próprio e também a "visão em tabela" acessível de qualquer gráfico. */
const props = defineProps<{ data: WidgetData; color?: (index: number, key?: string) => string }>();

const total = computed(() => props.data.points.reduce((sum, p) => sum + Math.abs(p.value ?? 0), 0));
const share = (value: number | null) =>
  total.value ? `${((Math.abs(value ?? 0) / total.value) * 100).toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%` : '—';
</script>

<template>
  <div class="table-wrap data-table">
    <table class="table">
      <thead>
        <tr>
          <th>{{ data.groupLabel ?? 'Categoria' }}</th>
          <th class="num">{{ data.measureLabel }}</th>
          <th class="num">Participação</th>
          <th class="num">Registros</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(p, i) in data.points" :key="p.key">
          <td>
            <span v-if="color" class="swatch" :style="{ background: color(i, p.key) }" />
            {{ p.label }}
          </td>
          <td class="num">{{ formatValue(p.value, data.format) }}</td>
          <td class="num">{{ share(p.value) }}</td>
          <td class="num">{{ p.count }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.data-table {
  height: 100%;
  overflow: auto;
}

.data-table .table th,
.data-table .table td {
  padding: 6px 12px;
  font-size: 0.85rem;
}

.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.swatch {
  display: inline-block;
  width: 10px;
  height: 10px;
  margin-right: 6px;
  border-radius: 3px;
  vertical-align: -1px;
}
</style>
