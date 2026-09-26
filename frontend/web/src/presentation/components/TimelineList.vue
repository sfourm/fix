<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import { useSessionStore } from '@/application/stores/session.store';
import { entityTypeLabel, timelineActionLabel } from '@/domain/labels';
import type { Member } from '@/domain/organization';
import type { Page } from '@/domain/page';
import type { TimelineEntry, TimelineFilters } from '@/domain/timeline';
import { useLoader } from '../composables/useAsync';
import { formatDateTime } from '../composables/format';
import StateBlock from './StateBlock.vue';
import { describeChanges } from './timeline-format';

/** Auditoria (tabela timelines do core). Sem filtros mostra a organização inteira. */
const props = defineProps<{
  entityType?: string;
  entityId?: string;
  limit?: number;
  /** Ação, autor, período e busca (tela de Auditoria). */
  filters?: TimelineFilters;
  /** Com página, a lista vem paginada e avisa o total por `paged`. */
  page?: number;
  pageSize?: number;
  members?: Member[] | null;
  refreshKey?: number;
  /** Mostra só as primeiras linhas de cada alteração (cockpit). */
  compact?: boolean;
}>();

const emit = defineEmits<{ paged: [info: Omit<Page<TimelineEntry>, 'items'>] }>();

const api = useApi();
const scope = () => ({ entityType: props.entityType, entityId: props.entityId, ...(props.filters ?? {}) });
const { data, loading, error, load } = useLoader(async () => {
  if (!props.page) return api.timeline.list({ ...scope(), limit: props.limit ?? 50 });
  const { items, ...info } = await api.timeline.page({ ...scope(), page: props.page, pageSize: props.pageSize ?? 25 });
  emit('paged', info);
  return items;
});

const session = useSessionStore();
const authors = computed(() => {
  const map = new Map((props.members ?? []).map((m) => [m.userId, m.fullName || m.email]));
  if (session.user) map.set(session.user.id, 'você');
  return map;
});

const actionClass: Record<TimelineEntry['action'], string> = {
  Created: 'badge-success',
  Updated: 'badge-info',
  Deleted: 'badge-danger',
};

function lines(entry: TimelineEntry): string[] {
  const all = describeChanges(entry);
  return props.compact && all.length > 3 ? [...all.slice(0, 3), `+ ${all.length - 3} campo(s)`] : all;
}

function author(entry: TimelineEntry): string {
  if (!entry.authorId) return 'Sistema';
  return authors.value.get(entry.authorId) ?? entry.authorId.slice(0, 8);
}

onMounted(load);
watch(() => [props.entityType, props.entityId, props.refreshKey, props.page, props.pageSize, JSON.stringify(props.filters ?? {})], load);
</script>

<template>
  <StateBlock
    :loading="loading && !data"
    :error="error"
    :empty="data?.length === 0"
    :empty-text="filters && Object.values(filters).some(Boolean) ? 'Nenhum registro com esses filtros.' : 'Nenhuma alteração registrada.'"
    @retry="load"
  >
    <ol class="timeline">
      <li v-for="entry in data ?? []" :key="entry.id">
        <div class="row">
          <span class="badge" :class="actionClass[entry.action]">{{ timelineActionLabel[entry.action] }}</span>
          <strong>{{ entityTypeLabel[entry.entityType] ?? entry.entityType }}</strong>
          <span class="muted small">por {{ author(entry) }} · {{ formatDateTime(entry.occurredAt) }}</span>
        </div>
        <ul class="changes small">
          <li v-for="line in lines(entry)" :key="line">{{ line }}</li>
        </ul>
      </li>
    </ol>
  </StateBlock>
</template>

<style scoped>

.timeline {
  list-style: none;
  margin: 0;
  padding: 4px 20px 16px;
}

.timeline > li {
  position: relative;
  padding: 12px 0 12px 20px;
  border-left: 2px solid var(--border);
}

.timeline > li::before {
  content: '';
  position: absolute;
  left: -6px;
  top: 18px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--primary);
}

.changes {
  margin: 6px 0 0;
  padding-left: 18px;
  color: var(--text-muted);
  word-break: break-word;
}
</style>
