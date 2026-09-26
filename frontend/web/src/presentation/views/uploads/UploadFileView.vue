<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { applyProgress, isFileRunning, PROCESSED_KINDS, type FileLine, type FileLineStatus, type UploadedFile } from '@/domain/file';
import { fileKindLabel, fileLineStatusLabel, fileLineStatusTone, fileStatusLabel, fileStatusTone } from '@/domain/labels';
import type { Page } from '@/domain/page';
import { ApiError, errorMessage } from '@/infrastructure/http/api-error';
import { useFileProgress } from '../../composables/useFileProgress';
import { useToast } from '../../composables/useToast';
import { formatDateTime, formatNumber } from '../../composables/format';
import FileProgressBar from '../../components/upload/FileProgressBar.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import { useTrailStore } from '../../trail';

const api = useApi();
const route = useRoute();
const toast = useToast();
const trail = useTrailStore();

const fileId = route.params.fileId as string;

const file = ref<UploadedFile | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const lines = ref<Page<FileLine> | null>(null);
const linesLoading = ref(false);
const filter = ref<FileLineStatus | null>(null);
const page = ref(1);
const pageSize = ref(50);

const processed = computed(() => !!file.value && PROCESSED_KINDS.includes(file.value.kind));
const running = computed(() => !!file.value && isFileRunning(file.value.status));

/** Colunas da planilha, na ordem em que vieram (da primeira linha carregada). */
const columns = computed(() => Object.keys(lines.value?.items[0]?.values ?? {}));

const filters = computed(() => {
  const f = file.value;
  return [
    { key: null, label: 'Todas', count: f?.totalLines ?? 0 },
    { key: 'Failed' as const, label: 'Com falha', count: f?.failedLines ?? 0 },
    { key: 'Succeeded' as const, label: 'Processadas', count: f?.succeededLines ?? 0 },
    { key: 'Pending' as const, label: 'Pendentes', count: Math.max(0, (f?.totalLines ?? 0) - (f?.processedLines ?? 0)) },
  ];
});

async function load() {
  loading.value = true;
  error.value = null;
  try {
    file.value = await api.files.get(fileId);
    trail.set(fileId, file.value.fileName);
    if (processed.value) await loadLines();
  } catch (e) {
    error.value = e instanceof ApiError && e.status === 404 ? 'Arquivo não encontrado.' : errorMessage(e);
  } finally {
    loading.value = false;
  }
}

async function loadLines() {
  linesLoading.value = true;
  try {
    lines.value = await api.files.lines(fileId, filter.value, { page: page.value, pageSize: pageSize.value });
  } catch (e) {
    toast.error(errorMessage(e));
  } finally {
    linesLoading.value = false;
  }
}

watch(filter, () => {
  page.value = 1;
  void loadLines();
});

async function download() {
  try {
    const blob = await api.files.content(fileId);
    const url = URL.createObjectURL(blob);
    Object.assign(document.createElement('a'), { href: url, download: file.value?.fileName ?? 'arquivo' }).click();
    URL.revokeObjectURL(url);
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// Ao vivo: os KPIs mudam a cada evento; as linhas recarregam agrupadas (no máximo uma vez por segundo).
let reloadTimer: ReturnType<typeof setTimeout> | undefined;
const { live } = useFileProgress(fileId, (event) => {
  if (!file.value) return;
  file.value = applyProgress(file.value, event);
  if (!reloadTimer) {
    reloadTimer = setTimeout(() => {
      reloadTimer = undefined;
      void loadLines();
    }, 1000);
  }
});

onMounted(load);
onBeforeUnmount(() => clearTimeout(reloadTimer));

const formatSize = (bytes: number) =>
  bytes >= 1024 * 1024 ? `${formatNumber(bytes / 1024 / 1024, 1)} MB` : `${formatNumber(Math.max(1, bytes / 1024), 0)} KB`;
</script>

<template>
  <StateBlock :loading="loading && !file" :error="error" @retry="load">
    <template v-if="file">
      <PageHeader :kicker="`Uploads · ${fileKindLabel[file.kind]}`" :title="file.fileName" :subtitle="`${formatSize(file.sizeBytes)} · enviado em ${formatDateTime(file.uploadedAt)}`">
        <template #meta>
          <div class="row">
            <StatusBadge :label="fileStatusLabel[file.status]" :tone="fileStatusTone[file.status]" />
            <span v-if="processed && running" class="badge" :class="live ? 'badge-success' : ''">{{ live ? '● ao vivo' : 'offline' }}</span>
          </div>
        </template>
        <template #actions>
          <button class="btn" type="button" @click="download">Baixar arquivo original</button>
        </template>
      </PageHeader>

      <p v-if="file.error" class="alert alert-error" style="margin-bottom: 16px">{{ file.error }}</p>

      <template v-if="processed">
        <section class="kpis" style="margin-bottom: 16px">
          <div class="kpi progress-kpi">
            <span>Progresso</span>
            <strong>{{ file.progressPercent }}%</strong>
            <FileProgressBar
              :percent="file.progressPercent"
              :succeeded="file.succeededLines"
              :failed="file.failedLines"
              :total="file.totalLines"
              :running="running"
            />
            <small>{{ file.processedLines }} de {{ file.totalLines }} linha(s)</small>
          </div>
          <div class="kpi">
            <span>Processadas</span>
            <strong class="ok">{{ file.succeededLines }}</strong>
            <small>refletiram na base</small>
          </div>
          <div class="kpi" :class="{ outside: file.failedLines > 0 }">
            <span>Com falha</span>
            <strong :class="{ fail: file.failedLines > 0 }">{{ file.failedLines }}</strong>
            <small>{{ file.failedLines ? 'corrija e reenvie só estas' : 'nenhuma' }}</small>
          </div>
          <div class="kpi">
            <span>Concluído em</span>
            <strong class="compact">{{ file.finishedAt ? formatDateTime(file.finishedAt) : '—' }}</strong>
            <small>{{ running ? 'em processamento' : fileStatusLabel[file.status] }}</small>
          </div>
        </section>

        <section class="card">
          <header class="card-header">
            <h2>Linhas</h2>
            <nav class="tabs line-tabs" role="tablist" aria-label="Filtrar linhas">
              <button
                v-for="f in filters"
                :key="f.key ?? 'all'"
                type="button"
                role="tab"
                class="tab"
                :class="{ active: filter === f.key }"
                :aria-selected="filter === f.key"
                @click="filter = f.key"
              >
                {{ f.label }} <span class="count">{{ f.count }}</span>
              </button>
            </nav>
          </header>
          <StateBlock
            :loading="linesLoading && !lines"
            :empty="!!lines && !lines.items.length"
            :empty-text="filter === 'Failed' ? 'Nenhuma linha com falha.' : 'Nenhuma linha neste filtro.'"
          >
            <div class="table-wrap">
              <table class="table lines">
                <thead>
                  <tr>
                    <th class="num">Linha</th>
                    <th>Status</th>
                    <th class="result">Resultado</th>
                    <th v-for="c in columns" :key="c" class="mono">{{ c }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="l in lines?.items" :key="l.id" :class="{ failed: l.status === 'Failed' }">
                    <td class="num">{{ l.number }}</td>
                    <td><StatusBadge :label="fileLineStatusLabel[l.status]" :tone="fileLineStatusTone[l.status]" /></td>
                    <td class="result">
                      <strong v-if="l.resultCode" class="mono">{{ l.resultCode }}</strong>
                      <span :class="{ 'fail-text': l.status === 'Failed' }">{{ l.message ?? (l.status === 'Pending' ? 'Aguardando…' : '—') }}</span>
                    </td>
                    <td v-for="c in columns" :key="c" class="value">{{ l.values[c] || '—' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <PaginationBar
              v-if="lines"
              :page="lines.page"
              :page-size="lines.pageSize"
              :total-count="lines.totalCount"
              :page-sizes="[25, 50, 100, 200]"
              @change="(p) => { page = p; loadLines(); }"
              @page-size="(s) => { pageSize = s; page = 1; loadLines(); }"
            />
          </StateBlock>
        </section>
      </template>

      <section v-else class="card">
        <dl class="details card-body">
          <dt>Tipo</dt>
          <dd>{{ fileKindLabel[file.kind] }} (só armazena — não é processado)</dd>
          <dt>Formato</dt>
          <dd class="mono">{{ file.contentType }}</dd>
          <dt>Tamanho</dt>
          <dd>{{ formatSize(file.sizeBytes) }}</dd>
          <dt>Enviado em</dt>
          <dd>{{ formatDateTime(file.uploadedAt) }}</dd>
        </dl>
      </section>
    </template>
  </StateBlock>
</template>

<style scoped>
.progress-kpi {
  gap: 0;
}

.progress-kpi .progress {
  margin-top: 10px;
}

.kpis .kpi > strong.ok {
  color: var(--success);
}

.kpis .kpi > strong.fail {
  color: var(--danger);
}

.kpis .kpi.outside {
  border-color: var(--danger-line);
}

.line-tabs {
  margin: 0;
  border: 0;
}

.lines td,
.lines th {
  white-space: nowrap;
}

.lines td.result {
  min-width: 280px;
  max-width: 460px;
  white-space: normal;
}

.lines td.result strong {
  margin-right: 8px;
}

.lines tr.failed td {
  background: color-mix(in srgb, var(--danger-soft) 55%, transparent);
}

.fail-text {
  color: var(--danger);
}

.value {
  max-width: 240px;
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--text-dim);
}
</style>
