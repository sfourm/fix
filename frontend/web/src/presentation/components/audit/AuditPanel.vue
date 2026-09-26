<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useSessionStore } from '@/application/stores/session.store';
import { entityTypeLabel, timelineActionLabel } from '@/domain/labels';
import type { Member } from '@/domain/organization';
import { Permission } from '@/domain/permissions';
import type { TimelineEntry, TimelineFilters } from '@/domain/timeline';
import { formatDate } from '../../composables/format';
import { useToast } from '../../composables/useToast';
import PaginationBar from '../PaginationBar.vue';
import TimelineList from '../TimelineList.vue';
import { exportAuditCsv, exportAuditPdf, MAX_ROWS } from './audit-export';

/**
 * Auditoria: alterações com busca, filtros (entidade, ação, autor, período), paginação e exportação (CSV/PDF).
 * Sem escopo mostra a organização inteira (tela Auditoria); com entityType/entityId, só aquele item (ex.: aba da
 * política). Filtros e página ficam na URL (dá para compartilhar a pesquisa ou voltar a ela).
 */
const props = defineProps<{
  entityType?: string;
  entityId?: string;
  /** Nome do escopo no PDF e no arquivo exportado (ex.: "POL-2026"). */
  scopeLabel?: string;
}>();

const api = useApi();
const organization = useOrganizationStore();
const session = useSessionStore();
const toast = useToast();
const route = useRoute();
const router = useRouter();

const members = ref<Member[] | null>(null);
const refreshKey = ref(0);

const ENTITY_TYPES = ['Organization', 'OrganizationGroup', 'Counterparty', 'Policy', 'Mandate', 'Order', 'Rule'];
const ACTIONS: TimelineEntry['action'][] = ['Created', 'Updated', 'Deleted'];
const PAGE_SIZES = [25, 50, 100, 200];

// ---------- Estado (espelhado na URL) ----------
const q = (key: string) => (typeof route.query[key] === 'string' ? (route.query[key] as string) : '');
const form = reactive({ search: q('q'), entity: q('entity'), action: q('action'), author: q('author'), from: q('from'), to: q('to') });
const showPanel = ref(Boolean(form.entity || form.action || form.author || form.from || form.to));
const page = ref(Math.max(1, Number(q('p')) || 1));
const pageSize = ref(PAGE_SIZES.includes(Number(q('size'))) ? Number(q('size')) : PAGE_SIZES[0]!);
const totalCount = ref(0);
const rangeStart = computed(() => (page.value - 1) * pageSize.value + 1);
const rangeEnd = computed(() => Math.min(page.value * pageSize.value, totalCount.value));

watch(
  () => ({ ...form, page: page.value, size: pageSize.value }),
  (f) => {
    const query = Object.fromEntries(
      Object.entries({
        q: f.search.trim(),
        entity: f.entity,
        action: f.action,
        author: f.author,
        from: f.from,
        to: f.to,
        p: f.page > 1 ? String(f.page) : '',
        size: f.size !== PAGE_SIZES[0] ? String(f.size) : '',
      }).filter(([, v]) => v),
    );
    router.replace({ query });
  },
);

/** Busca com atraso curto: não consulta a cada tecla. */
const search = ref(form.search.trim());
let timer: ReturnType<typeof setTimeout> | undefined;
watch(
  () => form.search,
  (value) => {
    clearTimeout(timer);
    timer = setTimeout(() => (search.value = value.trim()), 350);
  },
);

/** Data do filtro (dia local) → instante ISO; "até" inclui o dia inteiro. */
const startOf = (day: string) => new Date(`${day}T00:00:00`).toISOString();
const dayAfter = (day: string) => {
  const d = new Date(`${day}T00:00:00`);
  d.setDate(d.getDate() + 1);
  return d.toISOString();
};

/** Com escopo fixo (props), o filtro de entidade some e vale o item informado. */
const scoped = computed(() => Boolean(props.entityType));
const filters = computed<TimelineFilters & { entityType?: string; entityId?: string }>(() => ({
  entityType: props.entityType ?? (form.entity || undefined),
  entityId: props.entityId,
  search: search.value || undefined,
  action: (form.action as TimelineEntry['action']) || undefined,
  authorId: form.author || undefined,
  from: form.from ? startOf(form.from) : undefined,
  to: form.to ? dayAfter(form.to) : undefined,
}));

// Filtro ou tamanho de página novos começam da primeira página.
watch(
  () => [JSON.stringify(filters.value), pageSize.value],
  () => (page.value = 1),
);

// ---------- Atalhos de período ----------
const isoDay = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
const daysAgo = (n: number) => {
  const d = new Date();
  d.setDate(d.getDate() - n);
  return isoDay(d);
};
const PRESETS = [
  { key: 'all', label: 'Tudo', from: '', to: '' },
  { key: 'today', label: 'Hoje', from: daysAgo(0), to: '' },
  { key: '7d', label: '7 dias', from: daysAgo(6), to: '' },
  { key: '30d', label: '30 dias', from: daysAgo(29), to: '' },
];
const preset = computed(() => PRESETS.find((p) => p.from === form.from && p.to === form.to)?.key ?? null);
function applyPreset(p: (typeof PRESETS)[number]) {
  form.from = p.from;
  form.to = p.to;
}

// ---------- Filtros ativos (chips) ----------
const memberName = (userId: string) => {
  const m = members.value?.find((x) => x.userId === userId);
  return m ? m.fullName || m.email : 'membro';
};
const chips = computed(() =>
  [
    !scoped.value && form.entity && { key: 'entity', label: `Entidade: ${entityTypeLabel[form.entity] ?? form.entity}` },
    form.action && { key: 'action', label: `Ação: ${timelineActionLabel[form.action as TimelineEntry['action']]}` },
    form.author && { key: 'author', label: `Autor: ${memberName(form.author)}` },
    form.from && { key: 'from', label: `De ${formatDate(form.from)}` },
    form.to && { key: 'to', label: `Até ${formatDate(form.to)}` },
  ].filter((c): c is { key: 'entity' | 'action' | 'author' | 'from' | 'to'; label: string } => Boolean(c)),
);
const activeCount = computed(() => chips.value.length);

function clearAll() {
  Object.assign(form, { search: '', entity: '', action: '', author: '', from: '', to: '' });
  search.value = '';
}

// ---------- Exportação ----------
const exporting = ref<'csv' | 'pdf' | null>(null);
const exportMenu = ref(false);
const authorOf = (entry: TimelineEntry) =>
  !entry.authorId ? 'Sistema' : entry.authorId === session.user?.id ? session.user.fullName || 'você' : memberName(entry.authorId);
const filtersLabel = computed(
  () => [search.value && `busca "${search.value}"`, ...chips.value.map((c) => c.label)].filter(Boolean).join(' · ') || 'Sem filtros',
);

async function exportAs(format: 'csv' | 'pdf') {
  exportMenu.value = false;
  exporting.value = format;
  try {
    const input = {
      fetchPage: (p: number, size: number) => api.timeline.page({ ...filters.value, page: p, pageSize: size }),
      authorOf,
      filtersLabel: filtersLabel.value,
      organizationName: organization.current?.name ?? '',
      scopeLabel: props.scopeLabel,
    };
    const { exported, total } = await (format === 'csv' ? exportAuditCsv(input) : exportAuditPdf(input));
    if (total > exported) {
      toast.info(`Exportados os ${MAX_ROWS.toLocaleString('pt-BR')} registros mais recentes de ${total}. Refine os filtros para exportar o restante.`);
    } else {
      toast.success(`${exported} registro(s) exportado(s).`);
    }
  } catch (e) {
    console.error('Exportação da auditoria', e);
    toast.error('Não foi possível exportar.');
  } finally {
    exporting.value = null;
  }
}

onMounted(async () => {
  // Nomes dos autores (e o filtro por autor) só para quem pode ver os membros.
  if (organization.can(Permission.ViewUsers)) {
    members.value = await api.organizations.members().catch(() => null);
  }
});
</script>

<template>
  <section class="card">
    <!-- Barra de pesquisa -->
    <div class="toolbar">
      <label class="search">
        <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M11 18a7 7 0 1 0 0-14 7 7 0 0 0 0 14Zm5-2 4 4" /></svg>
        <input v-model="form.search" type="search" placeholder="Buscar por valor alterado, nome, código ou id…" aria-label="Buscar na auditoria" />
      </label>

      <div class="tabs presets" role="group" aria-label="Período">
        <button v-for="p in PRESETS" :key="p.key" type="button" class="tab" :class="{ active: preset === p.key }" @click="applyPreset(p)">{{ p.label }}</button>
      </div>

      <button type="button" class="btn" :class="{ 'btn-on': showPanel }" :aria-expanded="showPanel" aria-controls="audit-filters" @click="showPanel = !showPanel">
        <svg class="ico" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 6h16M7 12h10M10 18h4" /></svg>
        Filtros
        <span v-if="activeCount" class="badge badge-primary">{{ activeCount }}</span>
      </button>
      <div class="export">
        <button type="button" class="btn" :disabled="!!exporting || !totalCount" :aria-expanded="exportMenu" aria-haspopup="menu" @click="exportMenu = !exportMenu">
          <svg class="ico" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 4v11m0 0-4-4m4 4 4-4M5 19h14" /></svg>
          {{ exporting ? 'Exportando…' : 'Exportar' }}
        </button>
        <div v-if="exportMenu" class="export-menu" role="menu" @mouseleave="exportMenu = false">
          <button type="button" role="menuitem" @click="exportAs('csv')"><strong>Planilha (CSV)</strong><span>abre no Excel</span></button>
          <button type="button" role="menuitem" @click="exportAs('pdf')"><strong>PDF</strong><span>relatório para imprimir ou enviar</span></button>
          <p>Exporta todos os {{ totalCount }} registro(s) dos filtros atuais (até {{ MAX_ROWS.toLocaleString('pt-BR') }}).</p>
        </div>
      </div>
      <button type="button" class="btn btn-icon" title="Atualizar" aria-label="Atualizar" @click="refreshKey++">
        <svg class="ico" viewBox="0 0 24 24" aria-hidden="true"><path d="M20 11a8 8 0 1 0-2.3 5.7M20 5v6h-6" /></svg>
      </button>
    </div>

    <!-- Painel de filtros -->
    <Transition name="panel">
      <div v-if="showPanel" id="audit-filters" class="panel">
        <div class="form-grid wide">
          <div v-if="!scoped" class="field">
            <label for="f-entity">Entidade</label>
            <select id="f-entity" v-model="form.entity" class="input">
              <option value="">Todas</option>
              <option v-for="t in ENTITY_TYPES" :key="t" :value="t">{{ entityTypeLabel[t] ?? t }}</option>
            </select>
          </div>
          <div class="field">
            <label for="f-action">Ação</label>
            <select id="f-action" v-model="form.action" class="input">
              <option value="">Todas</option>
              <option v-for="a in ACTIONS" :key="a" :value="a">{{ timelineActionLabel[a] }}</option>
            </select>
          </div>
          <div class="field">
            <label for="f-author">Autor</label>
            <select id="f-author" v-model="form.author" class="input" :disabled="!members">
              <option value="">{{ members ? 'Todos' : 'Sem acesso aos membros' }}</option>
              <option v-for="m in members ?? []" :key="m.userId" :value="m.userId">{{ m.fullName || m.email }}</option>
            </select>
          </div>
          <div class="field period" :class="{ span2: scoped }">
            <label for="f-from">Período</label>
            <div class="row nowrap">
              <input id="f-from" v-model="form.from" class="input" type="date" :max="form.to || undefined" aria-label="De" />
              <span class="muted">a</span>
              <input id="f-to" v-model="form.to" class="input" type="date" :min="form.from || undefined" aria-label="Até" />
            </div>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Filtros ativos -->
    <div v-if="chips.length || search" class="chips">
      <span v-if="search" class="chip">
        Busca: “{{ search }}”
        <button type="button" aria-label="Remover busca" @click="form.search = ''; search = ''">×</button>
      </span>
      <span v-for="c in chips" :key="c.key" class="chip">
        {{ c.label }}
        <button type="button" :aria-label="`Remover ${c.label}`" @click="form[c.key] = ''">×</button>
      </span>
      <button type="button" class="btn btn-link small" @click="clearAll">Limpar tudo</button>
    </div>

    <!-- Resumo e itens por página (topo da lista) -->
    <div class="list-head">
      <span class="muted small">
        <template v-if="totalCount">{{ rangeStart }}–{{ rangeEnd }} de {{ totalCount }} registro(s)</template>
        <template v-else>Nenhum registro</template>
      </span>
      <label class="size muted small">
        Itens por página
        <select v-model.number="pageSize" class="input input-sm" aria-label="Itens por página">
          <option v-for="s in PAGE_SIZES" :key="s" :value="s">{{ s }}</option>
        </select>
      </label>
    </div>

    <TimelineList
      :filters="filters"
      :page="page"
      :page-size="pageSize"
      :members="members"
      :refresh-key="refreshKey"
      @paged="(info) => (totalCount = info.totalCount)"
    />
    <PaginationBar
      :page="page"
      :page-size="pageSize"
      :total-count="totalCount"
      @change="(p) => (page = p)"
    />
  </section>
</template>

<style scoped>
.toolbar {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  padding: 16px 20px;
}

.search {
  display: flex;
  flex: 1 1 320px;
  align-items: center;
  gap: 8px;
  min-width: 0;
  padding: 0 14px;
  border-radius: 999px;
  background: var(--fill);
  transition: box-shadow 0.2s var(--ease);
}

.search:focus-within {
  box-shadow: 0 0 0 4px var(--primary-soft);
}

.search svg,
.ico {
  flex: none;
  width: 16px;
  height: 16px;
  fill: none;
  stroke: currentcolor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.search svg {
  color: var(--text-muted);
}

.search input {
  flex: 1;
  min-width: 0;
  height: 38px;
  border: 0;
  background: transparent;
  color: var(--text);
  font: inherit;
  outline: none;
}

.presets {
  margin: 0;
}

.btn-on {
  background: var(--primary-soft);
  color: var(--primary);
}

.export {
  position: relative;
}

.export-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  z-index: 20;
  display: flex;
  flex-direction: column;
  gap: 2px;
  width: 270px;
  padding: 6px;
  border: 1px solid var(--border);
  border-radius: 14px;
  background: var(--surface-raised);
  box-shadow: var(--shadow-lg);
  animation: rise 0.18s var(--ease);
}

.export-menu button {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 1px;
  padding: 8px 10px;
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: var(--text);
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.export-menu button:hover {
  background: var(--fill);
}

.export-menu button span,
.export-menu p {
  color: var(--text-muted);
  font-size: 0.78rem;
}

.export-menu p {
  margin: 4px 10px 6px;
}

.panel {
  padding: 4px 20px 16px;
}

.nowrap {
  flex-wrap: nowrap;
}

.period .input {
  min-width: 0;
}

/* Sem o filtro de entidade sobra espaço: o período usa duas colunas. */
.period.span2 {
  grid-column: span 2;
}

.chips {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  padding: 0 20px 12px;
}

.chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 4px 3px 11px;
  border-radius: 999px;
  background: var(--primary-soft);
  color: var(--primary);
  font-size: 0.82rem;
  font-weight: 560;
}

.chip button {
  display: grid;
  place-items: center;
  width: 20px;
  height: 20px;
  border: 0;
  border-radius: 50%;
  background: transparent;
  color: inherit;
  font-size: 1rem;
  line-height: 1;
  cursor: pointer;
}

.chip button:hover {
  background: var(--primary-line);
}

.panel-enter-active,
.panel-leave-active {
  transition:
    opacity 0.2s var(--ease),
    transform 0.2s var(--ease);
}

.panel-enter-from,
.panel-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

.list-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  padding: 10px 20px;
  border-top: 1px solid var(--border);
}

.size {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

:deep(.timeline) {
  padding-top: 8px;
}

@media (max-width: 720px) {
  .period.span2 {
    grid-column: auto;
  }

  .toolbar {
    padding: 14px 16px;
  }

  .presets {
    order: 3;
    width: 100%;
  }

  .presets .tab {
    flex: 1;
    justify-content: center;
  }

  .panel,
  .chips,
  .list-head {
    padding-left: 16px;
    padding-right: 16px;
  }
}
</style>
