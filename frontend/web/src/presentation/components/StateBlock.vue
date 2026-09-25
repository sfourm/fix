<script setup lang="ts">
/** Estados de carregamento, erro e lista vazia num mesmo lugar. */
defineProps<{ loading?: boolean; error?: string | null; empty?: boolean; emptyText?: string }>();
defineEmits<{ retry: [] }>();
</script>

<template>
  <div v-if="loading" class="state"><span class="spinner" /> Carregando…</div>
  <div v-else-if="error" class="state">
    <span class="alert alert-error">{{ error }}</span>
    <button class="btn btn-sm" @click="$emit('retry')">Tentar novamente</button>
  </div>
  <div v-else-if="empty" class="state muted">{{ emptyText ?? 'Nenhum registro encontrado.' }}</div>
  <slot v-else />
</template>

<style scoped>
.state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 36px 20px;
  flex-wrap: wrap;
}
</style>
