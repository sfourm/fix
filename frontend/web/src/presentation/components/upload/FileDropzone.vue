<script setup lang="ts">
import { computed, ref } from 'vue';

/**
 * Área de envio: arrastar e soltar ou clicar para escolher. Confere extensão e tamanho antes de enviar; os arquivos
 * recusados voltam em `rejected` com o motivo.
 */
const props = defineProps<{
  /** Vazio = qualquer extensão. */
  extensions: string[];
  maxBytes: number;
  disabled?: boolean;
  busy?: boolean;
}>();
const emit = defineEmits<{ files: [files: File[]]; rejected: [rejected: { name: string; reason: string }[]] }>();

const input = ref<HTMLInputElement | null>(null);
const over = ref(false);

const accept = computed(() => props.extensions.join(','));
const hint = computed(() =>
  [props.extensions.length ? props.extensions.join(', ') : 'qualquer formato', `até ${Math.round(props.maxBytes / 1024 / 1024)} MB`].join(' · '),
);

function pick() {
  if (!props.disabled && !props.busy) input.value?.click();
}

function receive(list: FileList | null | undefined) {
  over.value = false;
  if (!list?.length || props.disabled || props.busy) return;

  const accepted: File[] = [];
  const reasons: { name: string; reason: string }[] = [];
  for (const file of Array.from(list)) {
    const extension = file.name.includes('.') ? file.name.slice(file.name.lastIndexOf('.')).toLowerCase() : '';
    if (props.extensions.length && !props.extensions.includes(extension)) {
      reasons.push({ name: file.name, reason: `Formato ${extension || 'sem extensão'} não aceito (use ${props.extensions.join(', ')}).` });
    } else if (file.size === 0) {
      reasons.push({ name: file.name, reason: 'O arquivo está vazio.' });
    } else if (file.size > props.maxBytes) {
      reasons.push({ name: file.name, reason: `Passa do limite de ${Math.round(props.maxBytes / 1024 / 1024)} MB.` });
    } else {
      accepted.push(file);
    }
  }

  if (reasons.length) emit('rejected', reasons);
  if (accepted.length) emit('files', accepted);
  if (input.value) input.value.value = '';
}
</script>

<template>
  <div
    class="dropzone"
    :class="{ over, disabled: disabled || busy }"
    role="button"
    :tabindex="disabled ? -1 : 0"
    :aria-disabled="disabled || busy"
    aria-label="Enviar arquivo: arraste para cá ou pressione Enter para escolher"
    @click="pick"
    @keydown.enter.prevent="pick"
    @keydown.space.prevent="pick"
    @dragenter.prevent="over = !disabled"
    @dragover.prevent="over = !disabled"
    @dragleave.prevent="over = false"
    @drop.prevent="receive($event.dataTransfer?.files)"
  >
    <input ref="input" type="file" multiple hidden :accept="accept" @change="receive(($event.target as HTMLInputElement).files)" />
    <svg class="icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 16V4m0 0-4.5 4.5M12 4l4.5 4.5" />
      <path d="M4 15v3a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3" />
    </svg>
    <strong v-if="busy">Enviando…</strong>
    <strong v-else-if="over">Solte para enviar</strong>
    <strong v-else>Arraste arquivos para cá ou <span class="link">clique para escolher</span></strong>
    <span class="muted small">{{ hint }}</span>
    <slot />
  </div>
</template>

<style scoped>
.dropzone {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  min-height: 168px;
  padding: 24px;
  border: 1.5px dashed var(--border-strong);
  border-radius: var(--radius);
  background: var(--surface-2);
  text-align: center;
  cursor: pointer;
  transition:
    border-color 0.2s var(--ease),
    background 0.2s var(--ease);
}

.dropzone:hover,
.dropzone:focus-visible,
.dropzone.over {
  border-color: var(--primary);
  background: var(--primary-soft);
  outline: none;
}

.dropzone.disabled {
  cursor: default;
  opacity: 0.6;
}

.icon {
  width: 30px;
  height: 30px;
  margin-bottom: 4px;
  fill: none;
  stroke: var(--primary);
  stroke-width: 1.6;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.link {
  color: var(--primary);
  text-decoration: underline;
  text-underline-offset: 3px;
}
</style>
