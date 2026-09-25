<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useCatalogStore } from '@/application/stores/catalog.store';
import { operatorLabel, visibilityLabel, VISIBILITIES, type DataSource, type FilterCriterion, type SavedFilter, type Scalar, type Visibility } from '@/domain/search';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useConfirm } from '../../composables/useConfirm';
import { useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';
import BaseModal from '../BaseModal.vue';
import FilterBuilder from './FilterBuilder.vue';

/**
 * Barra de filtros das listagens: escolhe um filtro salvo, edita condições avulsas e salva o resultado
 * como filtro (privado ou público na organização). Emite a pesquisa a aplicar.
 */
const props = defineProps<{ source: DataSource }>();
const emit = defineEmits<{ change: [search: { filterId: string | null; criteria: FilterCriterion[] }] }>();

const api = useApi();
const catalog = useCatalogStore();
const toast = useToast();
const { confirm } = useConfirm();

const saved = ref<SavedFilter[]>([]);
const selectedId = ref<string | null>(null);
const selected = computed(() => saved.value.find((f) => f.id === selectedId.value) ?? null);
const draft = ref<FilterCriterion[]>([]);
const applied = ref<FilterCriterion[]>([]);
const open = ref(false);

async function loadSaved() {
  try {
    saved.value = await api.filters.list(props.source);
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

function apply() {
  applied.value = draft.value.map((c) => ({ ...c }));
  open.value = false;
  emit('change', { filterId: selectedId.value, criteria: applied.value });
}

function selectFilter(id: string) {
  selectedId.value = id || null;
  apply();
}

function clear() {
  selectedId.value = null;
  draft.value = [];
  apply();
}

// ---------- Salvar ----------
const saving = ref<'new' | 'update' | null>(null);
const form = reactive<{ name: string; visibility: Visibility }>({ name: '', visibility: 'Private' });
const submit = useSubmit();

/** Salvar como novo junta as condições do filtro selecionado com as avulsas. */
function openSave(mode: 'new' | 'update') {
  form.name = mode === 'update' ? selected.value!.name : '';
  form.visibility = mode === 'update' ? selected.value!.visibility : 'Private';
  submit.reset();
  saving.value = mode;
}

async function save() {
  const mode = saving.value;
  const criteria = mode === 'update' ? draft.value : [...(selected.value?.criteria ?? []), ...draft.value];
  const result = await submit.run(() =>
    mode === 'update'
      ? api.filters.update(selectedId.value!, { name: form.name, criteria, visibility: form.visibility })
      : api.filters.create({ name: form.name, source: props.source, criteria, visibility: form.visibility }),
  );
  if (!result) return;

  saving.value = null;
  await loadSaved();
  selectedId.value = result.id;
  draft.value = [];
  apply();
  toast.success(mode === 'update' ? 'Filtro atualizado.' : 'Filtro salvo.');
}

/** Editar o filtro selecionado: as condições dele passam para o editor. */
function editSelected() {
  draft.value = selected.value!.criteria.map((c) => ({ ...c }));
  open.value = true;
}

async function removeSelected() {
  const filter = selected.value!;
  if (!(await confirm({ title: 'Excluir filtro', message: `Excluir o filtro "${filter.name}"?`, confirmLabel: 'Excluir', danger: true }))) return;
  try {
    await api.filters.remove(filter.id);
    toast.success('Filtro excluído.');
    await loadSaved();
    clear();
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

/** Resumo legível das condições aplicadas ("Aprovação é igual a Aprovada"). */
function describe(c: FilterCriterion): string {
  const field = catalog.field(props.source, c.field);
  const show = (v: Scalar) => field?.options.find((o) => o.value === v)?.label ?? (typeof v === 'boolean' ? (v ? 'Sim' : 'Não') : String(v));
  const value = c.value === null ? '' : Array.isArray(c.value) ? c.value.map(show).join(c.operator === 'between' ? ' e ' : ', ') : show(c.value);
  return `${field?.label ?? c.field} ${operatorLabel[c.operator]} ${value}`.trim();
}

onMounted(() => {
  catalog.load().catch((e) => toast.error(errorMessage(e)));
  loadSaved();
});
</script>

<template>
  <div class="filter-bar">
    <div class="row">
      <select class="input input-sm" :value="selectedId ?? ''" aria-label="Filtro salvo" @change="selectFilter(($event.target as HTMLSelectElement).value)">
        <option value="">Sem filtro salvo</option>
        <option v-for="f in saved" :key="f.id" :value="f.id">{{ f.name }}{{ f.visibility === 'Public' ? ' · público' : '' }}</option>
      </select>
      <button class="btn btn-sm" type="button" :aria-expanded="open" @click="open = !open">
        Condições{{ applied.length ? ` (${applied.length})` : '' }}
      </button>
      <template v-if="selected">
        <button v-if="selected.canEdit" class="btn btn-sm" type="button" @click="editSelected">Editar filtro</button>
        <button v-if="selected.canEdit" class="btn btn-sm btn-danger" type="button" @click="removeSelected">Excluir</button>
      </template>
      <button v-if="selectedId || applied.length" class="btn-link btn small" type="button" @click="clear">Limpar</button>
    </div>

    <div v-if="selected || applied.length" class="chips">
      <span v-for="c in selected?.criteria ?? []" :key="`s-${c.field}-${c.operator}`" class="badge badge-primary">{{ describe(c) }}</span>
      <span v-for="(c, i) in applied" :key="`a-${i}`" class="badge">{{ describe(c) }}</span>
    </div>

    <div v-if="open" class="panel card card-body stack">
      <p v-if="selected" class="muted small" style="margin: 0">
        As condições abaixo se somam às do filtro "{{ selected.name }}". Para mudar o próprio filtro, use "Editar filtro" e "Atualizar filtro".
      </p>
      <FilterBuilder v-model="draft" :source="source" />
      <div class="row">
        <button class="btn btn-primary btn-sm" type="button" @click="apply">Aplicar</button>
        <button class="btn btn-sm" type="button" :disabled="!draft.length && !selected" @click="openSave('new')">Salvar como filtro…</button>
        <button v-if="selected?.canEdit" class="btn btn-sm" type="button" :disabled="!draft.length" @click="openSave('update')">Atualizar "{{ selected.name }}"</button>
      </div>
    </div>

    <BaseModal v-if="saving" :title="saving === 'update' ? 'Atualizar filtro' : 'Salvar filtro'" width="440px" @close="saving = null">
      <form id="save-filter-form" class="stack" @submit.prevent="save">
        <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
        <div class="field">
          <label for="filter-name">Nome</label>
          <input id="filter-name" v-model="form.name" class="input" maxlength="120" required autofocus />
        </div>
        <div class="field">
          <label for="filter-visibility">Visibilidade</label>
          <select id="filter-visibility" v-model="form.visibility" class="input">
            <option v-for="v in VISIBILITIES" :key="v" :value="v">{{ visibilityLabel[v] }}</option>
          </select>
        </div>
      </form>
      <template #footer>
        <button class="btn" type="button" @click="saving = null">Cancelar</button>
        <button class="btn btn-primary" type="submit" form="save-filter-form" :disabled="submit.submitting.value">Salvar</button>
      </template>
    </BaseModal>
  </div>
</template>

<style scoped>
.filter-bar {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 12px;
}

.chips {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.panel {
  box-shadow: var(--shadow-lg);
}
</style>
