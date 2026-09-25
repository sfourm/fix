<script setup lang="ts">
defineProps<{ title: string; subtitle?: string | null }>();
</script>

<template>
  <header class="page-header">
    <div class="stack" style="gap: 4px">
      <slot name="breadcrumb" />
      <h1>{{ title }}</h1>
      <p v-if="subtitle" class="muted" style="margin: 0">{{ subtitle }}</p>
    </div>
    <div class="row">
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
  margin-bottom: 20px;
}
</style>
