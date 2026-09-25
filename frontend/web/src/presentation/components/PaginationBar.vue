<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{ page: number; pageSize: number; totalCount: number }>();
const emit = defineEmits<{ change: [page: number] }>();

const totalPages = computed(() => Math.max(1, Math.ceil(props.totalCount / props.pageSize)));
</script>

<template>
  <div v-if="totalCount > 0" class="pagination">
    <span class="muted small">{{ totalCount }} registro(s) · página {{ page }} de {{ totalPages }}</span>
    <div class="row">
      <button class="btn btn-sm" :disabled="page <= 1" @click="emit('change', page - 1)">Anterior</button>
      <button class="btn btn-sm" :disabled="page >= totalPages" @click="emit('change', page + 1)">Próxima</button>
    </div>
  </div>
</template>

<style scoped>
.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 20px;
  border-top: 1px solid var(--border);
}
</style>
