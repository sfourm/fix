<script setup lang="ts">
/**
 * Cabeçalho de página no padrão do FIX: kicker (contexto em caixa alta), título em serifa, texto de apoio e ações.
 * O caminho (breadcrumb) fica na barra superior do layout.
 */
defineProps<{ title: string; subtitle?: string | null; kicker?: string | null }>();
</script>

<template>
  <header class="page-header">
    <div class="head">
      <span v-if="kicker" class="kicker">{{ kicker }}</span>
      <h1>{{ title }}</h1>
      <p v-if="subtitle" class="lead">{{ subtitle }}</p>
      <slot name="meta" />
    </div>
    <div class="actions">
      <slot name="actions" />
    </div>
  </header>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
  margin-bottom: 18px;
}

.head {
  display: flex;
  flex-direction: column;
  gap: 5px;
  min-width: 0;
}

.head h1 {
  margin-bottom: 2px;
}

.actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
</style>
