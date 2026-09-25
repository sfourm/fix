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
      <div class="modal" :style="{ maxWidth: width ?? '560px' }" role="dialog" aria-modal="true" :aria-label="title">
        <header class="head">
          <h2>{{ title }}</h2>
          <button class="close" type="button" aria-label="Fechar" @click="emit('close')">
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6 6 18" /></svg>
          </button>
        </header>
        <div class="body">
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
  background: rgb(0 0 0 / 30%);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  animation: fade 0.25s var(--ease);
}

.modal {
  display: flex;
  flex-direction: column;
  width: 100%;
  max-height: calc(100dvh - 32px);
  border: 1px solid var(--border);
  border-radius: 24px;
  background: var(--surface);
  box-shadow: var(--shadow-lg);
  animation: pop 0.35s var(--ease);
}

.head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 20px 22px 8px;
}

.head h2 {
  font-size: 1.2rem;
}

.close {
  display: grid;
  place-items: center;
  flex: none;
  width: 30px;
  height: 30px;
  border: 0;
  border-radius: 50%;
  background: var(--fill);
  color: var(--text-muted);
  cursor: pointer;
  transition: background 0.2s var(--ease);
}

.close:hover {
  background: var(--fill-strong);
  color: var(--text);
}

.close svg {
  width: 14px;
  height: 14px;
  fill: none;
  stroke: currentcolor;
  stroke-width: 2;
  stroke-linecap: round;
}

.body {
  padding: 12px 22px 20px;
  overflow-y: auto;
}

.footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  padding: 14px 22px 18px;
  border-top: 1px solid var(--border);
}

@keyframes fade {
  from {
    opacity: 0;
  }
}

@keyframes pop {
  from {
    opacity: 0;
    transform: scale(0.96) translateY(8px);
  }
}

@keyframes sheet {
  from {
    transform: translateY(100%);
  }
}

/* Celular: vira uma "folha" que sobe da base da tela. */
@media (max-width: 640px) {
  .backdrop {
    place-items: end stretch;
    padding: 0;
  }

  .modal {
    max-width: none !important;
    max-height: 92dvh;
    border-radius: 24px 24px 0 0;
    border-bottom: 0;
    animation: sheet 0.4s var(--ease);
  }

  .head::before {
    content: '';
    position: absolute;
    top: 8px;
    left: 50%;
    width: 38px;
    height: 5px;
    margin-left: -19px;
    border-radius: 999px;
    background: var(--fill-strong);
  }

  .modal {
    position: relative;
  }

  .head {
    padding-top: 24px;
  }

  .body {
    padding: 12px 18px 18px;
  }

  .footer {
    padding: 12px 18px max(16px, env(safe-area-inset-bottom));
  }

  .footer :slotted(.btn) {
    flex: 1;
  }
}
</style>
