<script setup lang="ts">
import { computed } from 'vue';

/**
 * Paginação: total, páginas numeradas (com reticências quando são muitas) e, com `pageSizes`, o seletor de
 * itens por página.
 */
const props = defineProps<{ page: number; pageSize: number; totalCount: number; pageSizes?: number[] }>();
const emit = defineEmits<{ change: [page: number]; pageSize: [size: number] }>();

const totalPages = computed(() => Math.max(1, Math.ceil(props.totalCount / props.pageSize)));
const first = computed(() => (props.totalCount ? (props.page - 1) * props.pageSize + 1 : 0));
const last = computed(() => Math.min(props.page * props.pageSize, props.totalCount));

/** 1 … 4 5 [6] 7 8 … 20: sempre a primeira, a última e duas vizinhas da atual. */
const pages = computed<(number | '…')[]>(() => {
  const total = totalPages.value;
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const around = [props.page - 1, props.page, props.page + 1].filter((p) => p > 1 && p < total);
  const list: (number | '…')[] = [1];
  if (around[0]! > 2) list.push('…');
  list.push(...around);
  if (around[around.length - 1]! < total - 1) list.push('…');
  list.push(total);
  return list;
});

function go(page: number) {
  if (page >= 1 && page <= totalPages.value && page !== props.page) emit('change', page);
}
</script>

<template>
  <div v-if="totalCount > 0" class="pagination">
    <div class="row info">
      <span class="muted small">{{ first }}–{{ last }} de {{ totalCount }} registro(s)</span>
      <label v-if="pageSizes?.length" class="size muted small">
        Itens por página
        <select class="input input-sm" :value="pageSize" aria-label="Itens por página" @change="emit('pageSize', Number(($event.target as HTMLSelectElement).value))">
          <option v-for="s in pageSizes" :key="s" :value="s">{{ s }}</option>
        </select>
      </label>
    </div>
    <nav class="pages" aria-label="Páginas">
      <button class="pg" type="button" :disabled="page <= 1" aria-label="Página anterior" @click="go(page - 1)">‹</button>
      <template v-for="(p, i) in pages" :key="`${p}-${i}`">
        <span v-if="p === '…'" class="gap" aria-hidden="true">…</span>
        <button v-else class="pg" type="button" :class="{ on: p === page }" :aria-current="p === page ? 'page' : undefined" @click="go(p)">{{ p }}</button>
      </template>
      <button class="pg" type="button" :disabled="page >= totalPages" aria-label="Próxima página" @click="go(page + 1)">›</button>
    </nav>
  </div>
</template>

<style scoped>
.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  padding: 12px 20px;
  border-top: 1px solid var(--border);
}

.info {
  gap: 16px;
}

.size {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.pages {
  display: flex;
  align-items: center;
  gap: 4px;
}

.pg {
  min-width: 32px;
  height: 32px;
  padding: 0 8px;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: var(--text-dim);
  font: inherit;
  font-size: 0.88rem;
  font-weight: 560;
  font-variant-numeric: tabular-nums;
  cursor: pointer;
  transition:
    background 0.2s var(--ease),
    color 0.2s var(--ease);
}

.pg:hover:not(:disabled, .on) {
  background: var(--fill);
  color: var(--text);
}

.pg.on {
  background: var(--primary);
  color: var(--on-primary);
}

.pg:disabled {
  opacity: 0.35;
  cursor: default;
}

.gap {
  padding: 0 4px;
  color: var(--text-faint);
}

@media (max-width: 560px) {
  .pagination {
    justify-content: center;
  }

  .info {
    justify-content: center;
    width: 100%;
  }
}
</style>
