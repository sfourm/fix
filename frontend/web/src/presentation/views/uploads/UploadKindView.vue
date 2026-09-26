<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import {
  applyProgress,
  fileKindFromSlug,
  fileKindSlug,
  isFileRunning,
  MAX_UPLOAD_BYTES,
  type FileKind,
  type FileKindSummary,
  type FileTemplate,
  type UploadedFile,
} from '@/domain/file';
import { fileKindHint, fileKindLabel, fileStatusLabel, fileStatusTone } from '@/domain/labels';
import type { Page } from '@/domain/page';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useFileProgress } from '../../composables/useFileProgress';
import { useToast } from '../../composables/useToast';
import { formatDateTime, formatNumber } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import FileDropzone from '../../components/upload/FileDropzone.vue';
import FileProgressBar from '../../components/upload/FileProgressBar.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import { paths } from '../../paths';

const api = useApi();
const route = useRoute();
const router = useRouter();
const toast = useToast();

// A tela é remontada ao trocar de tipo (key do layout), então o tipo é fixo aqui.
const kind = fileKindFromSlug(route.params.kind as string) as FileKind;

const summary = ref<FileKindSummary | null>(null);
const template = ref<FileTemplate | null>(null);
const files = ref<Page<UploadedFile> | null>(null);
const page = ref(1);
const pageSize = ref(20);
const loading = ref(false);
const error = ref<string | null>(null);

/** Modal de envio e o resultado de cada arquivo enviado nela. */
const uploadOpen = ref(false);
const sent = ref<{ name: string; ok: boolean; message: string; fileId?: string }[]>([]);
const uploading = ref(false);

const processed = computed(() => template.value?.processed ?? false);
const canUpload = computed(() => summary.value?.canUpload ?? false);
const requiredColumns = computed(() => template.value?.columns.filter((c) => c.required).map((c) => c.name) ?? []);

async function loadFiles() {
  files.value = await api.files.list(kind, { page: page.value, pageSize: pageSize.value });
}

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const [kinds, tpl] = await Promise.all([api.files.summary(), api.files.template(kind), loadFiles()]);
    summary.value = kinds.find((k) => k.kind === kind) ?? null;
    template.value = tpl;
  } catch (e) {
    error.value = errorMessage(e);
  } finally {
    loading.value = false;
  }
}

function openUpload() {
  sent.value = [];
  uploadOpen.value = true;
}

async function upload(selected: File[]) {
  uploading.value = true;
  const results: typeof sent.value = [];
  try {
    for (const file of selected) {
      try {
        const saved = await api.files.upload(kind, file);
        results.push({ name: file.name, ok: true, fileId: saved.id, message: processed.value ? 'Recebido — processando as linhas.' : 'Armazenado.' });
      } catch (e) {
        results.push({ name: file.name, ok: false, message: errorMessage(e) });
      }
    }

    sent.value = [...results, ...sent.value];
    const ok = results.filter((r) => r.ok).length;
    if (ok) {
      toast.success(ok === 1 ? 'Arquivo enviado.' : `${ok} arquivos enviados.`);
      page.value = 1;
      await loadFiles();
    }

    // Tudo certo: fecha e o progresso segue na lista. Com recusa, a modal fica aberta mostrando o motivo.
    if (ok === results.length) uploadOpen.value = false;
  } finally {
    uploading.value = false;
  }
}

function rejected(list: { name: string; reason: string }[]) {
  sent.value = [...list.map((r) => ({ name: r.name, ok: false, message: r.reason })), ...sent.value];
}

async function downloadTemplate() {
  try {
    const blob = await api.files.templateCsv(kind);
    const url = URL.createObjectURL(blob);
    Object.assign(document.createElement('a'), { href: url, download: `modelo-${fileKindSlug[kind]}.csv` }).click();
    URL.revokeObjectURL(url);
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

function changePage(next: number) {
  page.value = next;
  void loadFiles();
}

function changePageSize(size: number) {
  pageSize.value = size;
  page.value = 1;
  void loadFiles();
}

// Ao vivo: atualiza a linha do arquivo na lista; arquivo novo (enviado por outra pessoa) recarrega a página 1.
const { live } = useFileProgress(null, (event) => {
  if (event.kind !== kind || !files.value) return;
  const index = files.value.items.findIndex((f) => f.id === event.fileId);
  if (index >= 0) {
    const items = [...files.value.items];
    items[index] = applyProgress(items[index]!, event);
    files.value = { ...files.value, items };
  } else if (page.value === 1) {
    void loadFiles();
  }
});

onMounted(() => {
  if (!kind) {
    void router.replace(paths.uploads());
    return;
  }

  void load().then(() => {
    // Atalho vindo da tela do contexto ("Importar planilha"): já abre a modal de envio.
    if (route.query.enviar === '1') {
      if (canUpload.value) openUpload();
      void router.replace({ query: {} });
    }
  });
});

const formatSize = (bytes: number) =>
  bytes >= 1024 * 1024 ? `${formatNumber(bytes / 1024 / 1024, 1)} MB` : `${formatNumber(Math.max(1, bytes / 1024), 0)} KB`;
</script>

<template>
  <template v-if="kind">
    <PageHeader kicker="Uploads" :title="fileKindLabel[kind]" :subtitle="fileKindHint[kind]">
      <template #meta>
        <div class="row">
          <span class="badge" :class="processed ? 'badge-primary' : ''">{{ processed ? 'Processado linha a linha' : 'Só armazena' }}</span>
          <span class="badge" :class="live ? 'badge-success' : ''">{{ live ? '● ao vivo' : 'offline' }}</span>
        </div>
      </template>
      <template #actions>
        <button
          class="btn btn-primary"
          type="button"
          :disabled="!canUpload"
          :title="canUpload ? '' : 'Você pode ver os arquivos, mas não tem a regra para enviar este tipo'"
          @click="openUpload"
        >
          Enviar arquivo
        </button>
      </template>
    </PageHeader>

    <StateBlock :loading="loading && !files" :error="error" @retry="load">
      <section class="card">
        <header class="card-header">
          <h2>Arquivos enviados</h2>
          <span class="muted small">{{ files?.totalCount ?? 0 }} arquivo(s)</span>
        </header>
        <StateBlock :empty="!files?.items.length" :empty-text="canUpload ? 'Nenhum arquivo enviado ainda. Use “Enviar arquivo”.' : 'Nenhum arquivo enviado ainda.'">
          <div class="table-wrap">
            <table class="table">
              <thead>
                <tr>
                  <th>Arquivo</th>
                  <th>Enviado em</th>
                  <th>Status</th>
                  <th v-if="processed" style="width: 26%">Progresso</th>
                  <th v-if="processed" class="num">Linhas</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="f in files?.items" :key="f.id" class="clickable" @click="router.push(paths.uploadFile(kind, f.id))">
                  <td>
                    <RouterLink :to="paths.uploadFile(kind, f.id)" @click.stop>{{ f.fileName }}</RouterLink>
                    <span class="sub">{{ formatSize(f.sizeBytes) }}</span>
                  </td>
                  <td>{{ formatDateTime(f.uploadedAt) }}</td>
                  <td><StatusBadge :label="fileStatusLabel[f.status]" :tone="fileStatusTone[f.status]" /></td>
                  <td v-if="processed">
                    <div class="progress-cell">
                      <FileProgressBar
                        :percent="f.progressPercent"
                        :succeeded="f.succeededLines"
                        :failed="f.failedLines"
                        :total="f.totalLines"
                        :running="isFileRunning(f.status)"
                      />
                      <span class="muted small num">{{ f.progressPercent }}%</span>
                    </div>
                  </td>
                  <td v-if="processed" class="num">
                    <span>{{ f.processedLines }}/{{ f.totalLines }}</span>
                    <span v-if="f.failedLines" class="sub fail">{{ f.failedLines }} com falha</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <PaginationBar
            v-if="files"
            :page="files.page"
            :page-size="files.pageSize"
            :total-count="files.totalCount"
            :page-sizes="[10, 20, 50]"
            @change="changePage"
            @page-size="changePageSize"
          />
        </StateBlock>
      </section>
    </StateBlock>

    <BaseModal v-if="uploadOpen" :title="`Enviar ${fileKindLabel[kind].toLowerCase()}`" :width="processed ? '720px' : '520px'" @close="uploadOpen = false">
      <div class="stack upload-modal">
        <FileDropzone
          :extensions="template?.acceptedExtensions ?? []"
          :max-bytes="MAX_UPLOAD_BYTES"
          :busy="uploading"
          @files="upload"
          @rejected="rejected"
        />

        <ul v-if="sent.length" class="sent">
          <li v-for="(s, i) in sent" :key="i" :class="s.ok ? 'ok' : 'fail'">
            <RouterLink v-if="s.fileId" :to="paths.uploadFile(kind, s.fileId)">{{ s.name }}</RouterLink>
            <strong v-else>{{ s.name }}</strong>
            <span>{{ s.message }}</span>
          </li>
        </ul>

        <section v-if="processed && template" class="template">
          <header class="row between">
            <h3>Modelo do arquivo</h3>
            <button class="btn btn-sm" type="button" @click="downloadTemplate">Baixar modelo CSV</button>
          </header>
          <p class="muted small">
            A primeira linha é o cabeçalho. Obrigatórias: <strong>{{ requiredColumns.join(', ') }}</strong>. Números com vírgula (16,42) e
            datas dd/mm/aaaa. No XML, cada <code>&lt;linha&gt;</code> tem um elemento por coluna.
          </p>
          <div class="table-wrap">
            <table class="table">
              <thead>
                <tr>
                  <th>Coluna</th>
                  <th>Descrição</th>
                  <th>Exemplo</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="c in template.columns" :key="c.name">
                  <td class="mono">
                    {{ c.name }}<span v-if="c.required" class="req" title="Obrigatória">*</span>
                  </td>
                  <td class="muted">{{ c.description }}</td>
                  <td class="mono muted">{{ c.example || '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </BaseModal>
  </template>
</template>

<style scoped>
.upload-modal {
  gap: 14px;
}

.template {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.template h3 {
  margin: 0;
  font-size: 1rem;
}

.template p {
  margin: 0;
}

.template .table-wrap {
  max-height: 260px;
  overflow: auto;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
}

.template td {
  vertical-align: top;
}

.between {
  justify-content: space-between;
}

.req {
  margin-left: 2px;
  color: var(--danger);
  font-weight: 700;
}

.sent {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin: 0;
  padding: 0;
  list-style: none;
}

.sent li {
  display: flex;
  flex-direction: column;
  gap: 1px;
  padding: 8px 12px;
  border-radius: var(--radius-sm);
  font-size: 0.86rem;
}

.sent li.ok {
  background: var(--success-soft);
}

.sent li.fail {
  background: var(--danger-soft);
}

.sent li.fail span {
  color: var(--danger);
}

.progress-cell {
  display: grid;
  grid-template-columns: 1fr 40px;
  align-items: center;
  gap: 10px;
}

.sub.fail {
  color: var(--danger);
}
</style>
