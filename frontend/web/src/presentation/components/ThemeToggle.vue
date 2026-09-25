<script setup lang="ts">
import { useThemeStore, type ThemeMode } from '@/application/stores/theme.store';

/** Seletor de tema no estilo segmentado (claro · escuro · automático). */
defineProps<{ compact?: boolean }>();
const theme = useThemeStore();

const options: { value: ThemeMode; label: string; icon: string }[] = [
  { value: 'light', label: 'Tema claro', icon: 'M12 7a5 5 0 1 0 0 10 5 5 0 0 0 0-10Zm0-5v2m0 16v2M4.2 4.2l1.4 1.4m12.8 12.8 1.4 1.4M2 12h2m16 0h2M4.2 19.8l1.4-1.4M18.4 5.6l1.4-1.4' },
  { value: 'dark', label: 'Tema escuro', icon: 'M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8Z' },
  { value: 'system', label: 'Automático (segue o sistema)', icon: 'M4 5h16v11H4zM8 20h8m-4-4v4' },
];
</script>

<template>
  <div class="segmented" :class="{ compact }" role="radiogroup" aria-label="Tema">
    <button
      v-for="o in options"
      :key="o.value"
      type="button"
      role="radio"
      :aria-checked="theme.mode === o.value"
      :aria-label="o.label"
      :title="o.label"
      :class="{ on: theme.mode === o.value }"
      @click="theme.set(o.value)"
    >
      <svg viewBox="0 0 24 24" aria-hidden="true"><path :d="o.icon" /></svg>
    </button>
  </div>
</template>

<style scoped>
.segmented {
  display: inline-flex;
  gap: 2px;
  padding: 3px;
  border-radius: 999px;
  background: var(--fill);
}

button {
  display: grid;
  place-items: center;
  width: 30px;
  height: 26px;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  transition:
    background 0.2s var(--ease),
    color 0.2s var(--ease),
    box-shadow 0.2s var(--ease);
}

button:hover {
  color: var(--text);
}

button.on {
  background: var(--surface-raised);
  color: var(--text);
  box-shadow: 0 1px 3px rgb(0 0 0 / 12%), 0 0 0 0.5px rgb(0 0 0 / 4%);
}

svg {
  width: 15px;
  height: 15px;
  fill: none;
  stroke: currentcolor;
  stroke-width: 1.8;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.compact button {
  width: 28px;
  height: 24px;
}
</style>
