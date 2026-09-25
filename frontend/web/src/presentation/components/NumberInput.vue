<script setup lang="ts">
/** Campo numérico que trabalha com number | null (vazio = null), no lugar do "" do v-model.number. */
const model = defineModel<number | null>({ required: true });
defineProps<{ id?: string; step?: string; min?: number; max?: number; invalid?: boolean; disabled?: boolean; required?: boolean }>();

function onInput(event: Event) {
  const raw = (event.target as HTMLInputElement).value;
  model.value = raw === '' ? null : Number(raw);
}
</script>

<template>
  <input
    :id="id"
    class="input"
    :class="{ invalid }"
    type="number"
    inputmode="decimal"
    :step="step ?? 'any'"
    :min="min"
    :max="max"
    :disabled="disabled"
    :required="required"
    :value="model ?? ''"
    @input="onInput"
  />
</template>
