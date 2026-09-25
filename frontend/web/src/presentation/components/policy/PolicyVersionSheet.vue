<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import { useSessionStore } from '@/application/stores/session.store';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import type { Policy } from '@/domain/policy';
import type { TimelineEntry } from '@/domain/timeline';
import { useLoader } from '../../composables/useAsync';
import { formatDate, formatDateTime } from '../../composables/format';
import { useToast } from '../../composables/useToast';
import BaseModal from '../BaseModal.vue';
import StateBlock from '../StateBlock.vue';
import StatusBadge from '../StatusBadge.vue';
import { changesByVersion, sectionLabel, timedVersions, type Change, type ChangeSection, type TimedVersion } from './policy-changes';

/** Modal de uma versão da política: etapas (rascunho → aprovação → vigência), o que mudou nela e exportação em PDF. */
const props = defineProps<{
  policy: Policy;
  version: string;
  /** Nome de quem fez a alteração (userId → nome), quando a tela tiver a lista de membros. */
  authorName?: (userId: string) => string | null;
}>();
const emit = defineEmits<{ close: []; select: [version: string] }>();

const api = useApi();
const session = useSessionStore();
const timeline = useLoader<TimelineEntry[]>(() => api.timeline.list({ entityType: 'Policy', entityId: props.policy.id, limit: 500 }));

/** Versões na ordem em que foram abertas, sem repetir (cada uma tem várias etapas). */
const versionList = computed(() => [...new Set(props.policy.versions.map((v) => v.version))]);
const lastStep = computed(() => steps.value[steps.value.length - 1] ?? null);
const index = computed(() => versionList.value.indexOf(props.version));
const previous = computed(() => versionList.value[index.value - 1] ?? null);
const next = computed(() => versionList.value[index.value + 1] ?? null);

/** Etapas da versão com data e hora; se já veio outra depois, a vigente desta termina como substituída. */
const steps = computed<TimedVersion[]>(() => {
  const all = timedVersions(props.policy.versions, timeline.data.value);
  const own = all.filter((v) => v.version === props.version);
  const successor = next.value ? all.find((v) => v.version === next.value) : null;
  return successor
    ? [...own, { version: props.version, status: 'Superseded', date: successor.date, at: successor.at, note: `substituída pela ${next.value}` }]
    : own;
});
const when = (s: TimedVersion) => (s.at ? formatDateTime(s.at) : formatDate(s.date));

const bucket = computed(() => (timeline.data.value ? changesByVersion(timeline.data.value).get(props.version) ?? null : null));

const ORDER: ChangeSection[] = ['header', 'limits', 'axes', 'bands', 'instruments'];
const sections = computed(() =>
  ORDER.map((s) => ({ key: s, label: sectionLabel[s], items: (bucket.value?.changes ?? []).filter((c) => c.section === s) })).filter((s) => s.items.length),
);
const total = computed(() => bucket.value?.changes.length ?? 0);
const counts = computed(() => {
  const all = bucket.value?.changes ?? [];
  return { added: all.filter((c) => c.kind === 'added').length, updated: all.filter((c) => c.kind === 'updated').length, removed: all.filter((c) => c.kind === 'removed').length };
});

/** Seções longas (a primeira versão traz a política inteira) começam recolhidas. */
const open = ref(new Set<ChangeSection>());
watch(
  sections,
  (list) => (open.value = new Set(list.filter((s) => s.items.length <= 6).map((s) => s.key))),
  { immediate: true },
);
function toggle(s: ChangeSection) {
  const next = new Set(open.value);
  if (next.has(s)) next.delete(s);
  else next.add(s);
  open.value = next;
}

const kindLabel: Record<Change['kind'], string> = { added: 'incluído', updated: 'alterado', removed: 'removido' };
const kindTone: Record<Change['kind'], string> = { added: 'badge-success', updated: 'badge-info', removed: 'badge-danger' };

function author(change: Change) {
  if (!change.authorId) return 'Sistema';
  if (change.authorId === session.user?.id) return 'você';
  return props.authorName?.(change.authorId) ?? 'outro membro';
}

const toast = useToast();
const exporting = ref(false);
async function exportPdf() {
  exporting.value = true;
  try {
    const { exportVersionPdf } = await import('./policy-pdf');
    await exportVersionPdf({
      policy: props.policy,
      version: props.version,
      steps: steps.value,
      changes: bucket.value?.changes ?? [],
      initial: bucket.value?.initial ?? false,
      previous: previous.value,
      authorOf: author,
    });
  } catch (e) {
    console.error('PDF da versão', e);
    toast.error('Não foi possível gerar o PDF.');
  } finally {
    exporting.value = false;
  }
}

onMounted(timeline.load);
</script>

<template>
  <BaseModal :title="`Versão ${version}`" width="760px" @close="emit('close')">
    <div class="stack">
      <div class="meta">
        <span class="kicker">{{ policy.code.toUpperCase() }} · {{ policy.title }}</span>
        <div class="row">
          <StatusBadge v-if="lastStep" :label="policyStatusLabel[lastStep.status]" :tone="policyStatusTone[lastStep.status]" />
          <span class="muted small">{{ total }} alteração(ões) de conteúdo</span>
        </div>
      </div>

      <!-- Navegação entre versões -->
      <nav class="versions" aria-label="Versões">
        <button
          v-for="v in versionList"
          :key="v"
          type="button"
          class="vchip"
          :class="{ on: v === version }"
          :aria-current="v === version ? 'true' : undefined"
          @click="emit('select', v)"
        >
          {{ v }}
        </button>
      </nav>

      <!-- Etapas da versão -->
      <section>
        <h3 class="sec-title">Ciclo da versão</h3>
        <ol class="steps">
          <li v-for="(s, i) in steps" :key="i" :class="`tone-${policyStatusTone[s.status]}`">
            <span class="dot" />
            <div>
              <strong>{{ policyStatusLabel[s.status] }}</strong>
              <span class="muted small"> · {{ when(s) }}</span>
              <div v-if="s.note" class="muted small">{{ s.note }}</div>
            </div>
          </li>
        </ol>
      </section>

      <!-- Alterações -->
      <section>
        <div class="row between">
          <h3 class="sec-title">{{ bucket?.initial ? 'Conteúdo inicial' : `O que mudou${previous ? ` desde ${previous}` : ''}` }}</h3>
          <div v-if="total" class="row counts small">
            <span v-if="counts.added" class="badge badge-success">+{{ counts.added }}</span>
            <span v-if="counts.updated" class="badge badge-info">~{{ counts.updated }}</span>
            <span v-if="counts.removed" class="badge badge-danger">−{{ counts.removed }}</span>
          </div>
        </div>

        <StateBlock :loading="timeline.loading.value && !timeline.data.value" :error="timeline.error.value" @retry="timeline.load">
          <p v-if="!total" class="alert alert-info">
            Nenhuma alteração de conteúdo nesta versão: ela foi aberta e aprovada com os mesmos parâmetros, eixos, bandas e instrumentos de
            {{ previous ?? 'antes' }}.
          </p>
          <p v-else-if="bucket?.initial" class="muted small intro">Primeira versão: a política nasce com o conteúdo abaixo (modelo FIX ou cadastro inicial) e os ajustes feitos até a aprovação.</p>

          <div v-for="s in sections" :key="s.key" class="group">
            <button type="button" class="group-head" :aria-expanded="open.has(s.key)" @click="toggle(s.key)">
              <span>{{ s.label }}</span>
              <span class="muted small">{{ s.items.length }}</span>
              <svg viewBox="0 0 24 24" aria-hidden="true" :class="{ rot: open.has(s.key) }"><path d="M9 6l6 6-6 6" /></svg>
            </button>
            <ul v-if="open.has(s.key)" class="changes">
              <li v-for="c in s.items" :key="c.id" class="change">
                <div class="row between">
                  <strong class="subject">{{ c.subject }}</strong>
                  <span class="badge" :class="kindTone[c.kind]">{{ kindLabel[c.kind] }}</span>
                </div>
                <dl v-if="c.lines.length" class="lines">
                  <template v-for="(l, i) in c.lines" :key="i">
                    <dt v-if="l.label">{{ l.label }}</dt>
                    <dd :class="{ full: !l.label }">
                      <template v-if="l.old !== undefined">
                        <span class="old">{{ l.old }}</span>
                        <span class="arrow" aria-label="passou para">→</span>
                      </template>
                      <span class="new">{{ l.new }}</span>
                    </dd>
                  </template>
                </dl>
                <span class="muted small by">{{ author(c) }} · {{ formatDateTime(c.occurredAt) }}</span>
              </li>
            </ul>
          </div>
        </StateBlock>
      </section>
    </div>

    <template #footer>
      <button class="btn" type="button" :disabled="!previous" @click="previous && emit('select', previous)">← {{ previous ?? 'Anterior' }}</button>
      <button class="btn" type="button" :disabled="!next" @click="next && emit('select', next)">{{ next ?? 'Próxima' }} →</button>
      <span class="spacer" />
      <button class="btn btn-primary" type="button" :disabled="exporting || !timeline.data.value" @click="exportPdf">
        {{ exporting ? 'Gerando…' : 'Exportar PDF' }}
      </button>
    </template>
  </BaseModal>
</template>

<style scoped>
.meta {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: -6px;
}

.spacer {
  flex: 1;
}

.between {
  justify-content: space-between;
}

.versions {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.vchip {
  padding: 5px 13px;
  border: 0;
  border-radius: 999px;
  background: var(--fill);
  color: var(--text-dim);
  font: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  cursor: pointer;
  transition:
    background 0.2s var(--ease),
    color 0.2s var(--ease);
}

.vchip:hover {
  background: var(--fill-strong);
}

.vchip.on {
  background: var(--primary);
  color: var(--on-primary);
}

.sec-title {
  margin: 0 0 10px;
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--text-muted);
}

.steps {
  margin: 0;
  padding: 0;
  list-style: none;
}

.steps li {
  position: relative;
  display: flex;
  gap: 12px;
  padding: 0 0 14px;
}

.steps li:not(:last-child)::before {
  content: '';
  position: absolute;
  top: 14px;
  bottom: 0;
  left: 5px;
  width: 2px;
  background: var(--border-strong);
}

.dot {
  flex: none;
  width: 12px;
  height: 12px;
  margin-top: 4px;
  border-radius: 50%;
  background: var(--text-faint);
}

.tone-success .dot {
  background: var(--success);
}

.tone-warning .dot {
  background: var(--warning);
}

.tone-info .dot {
  background: var(--info);
}

.intro {
  margin: 0 0 10px;
}

.group {
  border-top: 1px solid var(--border);
}

.group-head {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  padding: 12px 2px;
  border: 0;
  background: none;
  color: var(--text);
  font: inherit;
  font-weight: 600;
  text-align: left;
  cursor: pointer;
}

.group-head span:first-child {
  flex: 1;
}

.group-head svg {
  width: 16px;
  height: 16px;
  fill: none;
  stroke: var(--text-muted);
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
  transition: transform 0.25s var(--ease);
}

.group-head svg.rot {
  transform: rotate(90deg);
}

.changes {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin: 0 0 14px;
  padding: 0;
  list-style: none;
}

.change {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px 14px;
  border-radius: 14px;
  background: var(--surface-2);
}

.subject {
  min-width: 0;
  font-size: 0.92rem;
}

.lines {
  display: grid;
  grid-template-columns: minmax(90px, 38%) 1fr;
  gap: 4px 12px;
  margin: 0;
  font-size: 0.86rem;
}

.lines dt {
  color: var(--text-muted);
}

.lines dd {
  margin: 0;
  overflow-wrap: anywhere;
}

.lines dd.full {
  grid-column: 1 / -1;
}

.old {
  color: var(--text-muted);
  text-decoration: line-through;
  text-decoration-color: var(--danger-line);
}

.arrow {
  margin: 0 6px;
  color: var(--text-faint);
}

.new {
  font-weight: 560;
}

.by {
  font-size: 0.76rem;
}

@media (max-width: 480px) {
  .lines {
    grid-template-columns: 1fr;
  }
}
</style>
