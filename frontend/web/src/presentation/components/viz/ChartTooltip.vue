<script setup lang="ts">
/** Dica flutuante das marcas: segue o ponteiro, fica dentro do gráfico e usa as cores de texto (não a da série). */
defineProps<{ x: number; y: number; width: number; title: string; value: string; detail?: string; color?: string }>();
</script>

<template>
  <div class="tooltip" role="status" :style="{ left: `${Math.min(Math.max(x, 70), width - 70)}px`, top: `${y}px` }">
    <div class="row" style="gap: 6px; flex-wrap: nowrap">
      <span v-if="color" class="swatch" :style="{ background: color }" />
      <span class="title">{{ title }}</span>
    </div>
    <strong>{{ value }}</strong>
    <span v-if="detail" class="detail">{{ detail }}</span>
  </div>
</template>

<style scoped>
.tooltip {
  position: absolute;
  z-index: 5;
  transform: translate(-50%, calc(-100% - 10px));
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 110px;
  max-width: 220px;
  padding: 8px 10px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  box-shadow: var(--shadow-lg);
  font-size: 0.8rem;
  pointer-events: none;
}

.title {
  color: var(--text-muted);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.detail {
  color: var(--text-muted);
}

.swatch {
  flex: none;
  width: 10px;
  height: 10px;
  border-radius: 3px;
}
</style>
