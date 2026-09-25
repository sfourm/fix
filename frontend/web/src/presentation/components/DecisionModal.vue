<script setup lang="ts">
import { ref } from 'vue';
import { useSubmit } from '../composables/useAsync';
import BaseModal from './BaseModal.vue';

/**
 * Decisão com justificativa (aprovar, rejeitar, encerrar, divergência...). A ação recebe a nota
 * e o modal só fecha quando ela conclui; erros do core aparecem no próprio modal.
 */
const props = defineProps<{
  title: string;
  message?: string;
  noteLabel?: string;
  noteRequired?: boolean;
  confirmLabel: string;
  danger?: boolean;
  action: (note: string | null) => Promise<unknown>;
}>();
const emit = defineEmits<{ close: []; done: [] }>();

const note = ref('');
const { submitting, error, run } = useSubmit();

async function submit() {
  const done = await run(async () => {
    await props.action(note.value.trim() || null);
    return true;
  });
  if (done) emit('done');
}
</script>

<template>
  <BaseModal :title="title" width="480px" @close="emit('close')">
    <form id="decision-form" class="stack" @submit.prevent="submit">
      <p v-if="message" style="margin: 0">{{ message }}</p>
      <p v-if="error" class="alert alert-error">{{ error }}</p>
      <div class="field">
        <label for="decision-note">{{ noteLabel ?? (noteRequired ? 'Justificativa' : 'Observação (opcional)') }}</label>
        <textarea id="decision-note" v-model="note" class="input" maxlength="1000" :required="noteRequired" autofocus />
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="emit('close')">Cancelar</button>
      <button class="btn" :class="danger ? 'btn-danger' : 'btn-primary'" type="submit" form="decision-form" :disabled="submitting">
        {{ submitting ? 'Enviando…' : confirmLabel }}
      </button>
    </template>
  </BaseModal>
</template>
