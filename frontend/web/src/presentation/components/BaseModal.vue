<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue';

defineProps<{ title: string; width?: string }>();
const emit = defineEmits<{ close: [] }>();

function onKey(event: KeyboardEvent) {
  if (event.key === 'Escape') emit('close');
}

onMounted(() => document.addEventListener('keydown', onKey));
onBeforeUnmount(() => document.removeEventListener('keydown', onKey));
</script>

<template>
  <Teleport to="body">
    <div class="backdrop" @mousedown.self="emit('close')">
      <div class="modal card" :style="{ maxWidth: width ?? '560px' }" role="dialog" aria-modal="true" :aria-label="title">
        <header class="card-header">
          <h2>{{ title }}</h2>
          <button class="btn btn-sm" type="button" aria-label="Fechar" @click="emit('close')">✕</button>
        </header>
        <div class="card-body">
          <slot />
        </div>
        <footer v-if="$slots.footer" class="footer">
          <slot name="footer" />
        </footer>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.backdrop {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: grid;
  place-items: center;
  padding: 16px;
  background: rgb(10 12 20 / 55%);
}

.modal {
  width: 100%;
  max-height: calc(100vh - 32px);
  overflow: auto;
  box-shadow: var(--shadow-lg);
}

.footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  padding: 14px 20px;
  border-top: 1px solid var(--border);
}
</style>
