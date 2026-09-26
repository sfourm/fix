<script setup lang="ts">
import { computed } from 'vue';

/**
 * Barra do processamento: a parte verde são as linhas processadas, a vermelha as que falharam (proporcionais ao
 * total). Sem total (arquivo só armazenado ou ainda na fila), mostra só o percentual.
 */
const props = defineProps<{ percent: number; succeeded?: number; failed?: number; total?: number; running?: boolean }>();

const okWidth = computed(() => (props.total ? ((props.succeeded ?? 0) * 100) / props.total : props.percent));
const failWidth = computed(() => (props.total ? ((props.failed ?? 0) * 100) / props.total : 0));
</script>

<template>
  <div
    class="progress"
    :class="{ running }"
    role="progressbar"
    :aria-valuenow="percent"
    aria-valuemin="0"
    aria-valuemax="100"
    :aria-label="`${percent}% processado`"
  >
    <span class="ok" :style="{ width: `${okWidth}%` }" />
    <span class="fail" :style="{ width: `${failWidth}%` }" />
  </div>
</template>

<style scoped>
.progress {
  display: flex;
  gap: 2px;
  height: 8px;
  overflow: hidden;
  border-radius: 999px;
  background: var(--fill);
}

.progress span {
  height: 100%;
  border-radius: 999px;
  transition: width 0.4s var(--ease);
}

.ok {
  background: var(--success);
}

.fail {
  background: var(--danger);
}

/* Em andamento: brilho correndo sobre a barra. */
.running {
  position: relative;
}

.running::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, transparent, color-mix(in srgb, var(--surface) 55%, transparent), transparent);
  animation: sweep 1.4s linear infinite;
}

@keyframes sweep {
  from {
    transform: translateX(-100%);
  }

  to {
    transform: translateX(100%);
  }
}

@media (prefers-reduced-motion: reduce) {
  .running::after {
    animation: none;
  }
}
</style>
