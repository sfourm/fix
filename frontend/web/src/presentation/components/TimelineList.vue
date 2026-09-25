<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import { useSessionStore } from '@/application/stores/session.store';
import { childEntityLabel, entityTypeLabel, timelineActionLabel } from '@/domain/labels';
import type { Member } from '@/domain/organization';
import type { TimelineEntry } from '@/domain/timeline';
import { useLoader } from '../composables/useAsync';
import { formatDateTime } from '../composables/format';
import StateBlock from './StateBlock.vue';

/** Histórico de auditoria (tabela timelines do core). Sem filtros mostra a organização inteira. */
const props = defineProps<{
  entityType?: string;
  entityId?: string;
  limit?: number;
  members?: Member[] | null;
  refreshKey?: number;
  /** Mostra só as primeiras linhas de cada alteração (cockpit). */
  compact?: boolean;
}>();

const api = useApi();
const { data, loading, error, load } = useLoader(() =>
  api.timeline.list({ entityType: props.entityType, entityId: props.entityId, limit: props.limit ?? 50 }),
);

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

const fieldLabel: Record<string, string> = {
  Title: 'Título',
  Name: 'Nome',
  Slug: 'Identificador',
  Code: 'Código',
  Version: 'Versão',
  Description: 'Descrição',
  Status: 'Status',
  Approval: 'Aprovação',
  Confirmation: 'Confirmation',
  Desk: 'Mesa',
  IsDefault: 'Padrão',
  IsHomologated: 'Homologada',
  ApprovalRecord: 'Ata',
  DecisionNote: 'Justificativa',
  Consumed: 'Consumido',
};

/** 'PolicyAxis.Limits.Title' -> 'Eixo · Limits · Título' (prefixo = entidade filha do agregado). */
function labelOf(field: string): string {
  return field
    .split('.')
    .map((part, index) => (index === 0 && childEntityLabel[part]) || fieldLabel[part] || part)
    .join(' · ');
}

/** Campos de negócio alterados; ids técnicos (chaves estrangeiras) ficam de fora. */
function describe(entry: TimelineEntry): string[] {
  return Object.entries(entry.changes)
    .filter(([field]) => !field.endsWith('Id') && !field.endsWith('.Id'))
    .map(([field, value]) => {
      const label = labelOf(field);
      if (value && typeof value === 'object' && 'new' in value) {
        const change = value as { old: unknown; new: unknown };
        return `${label}: ${show(change.old)} → ${show(change.new)}`;
      }

      return `${label}: ${show(value)}`;
    });
}

function lines(entry: TimelineEntry): string[] {
  const all = describe(entry);
  return props.compact && all.length > 3 ? [...all.slice(0, 3), `+ ${all.length - 3} campo(s)`] : all;
}

function show(value: unknown): string {
  if (value === null || value === undefined || value === '') return '∅';
  if (typeof value === 'boolean') return value ? 'sim' : 'não';
  return typeof value === 'object' ? JSON.stringify(value) : String(value);
}

function author(entry: TimelineEntry): string {
  if (!entry.authorId) return 'Sistema';
  return authors.value.get(entry.authorId) ?? entry.authorId.slice(0, 8);
}

onMounted(load);
watch(() => [props.entityType, props.entityId, props.refreshKey], load);
</script>

<template>
  <StateBlock :loading="loading && !data" :error="error" :empty="data?.length === 0" empty-text="Nenhuma alteração registrada." @retry="load">
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
