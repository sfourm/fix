<script setup lang="ts">
import { computed } from 'vue';
import { useCatalogStore } from '@/application/stores/catalog.store';
import { operatorLabel, type DataSource, type DataSourceField, type FilterCriterion, type FilterOperator, type Scalar } from '@/domain/search';

/** Editor de condições (E lógico) montado a partir do catálogo do conjunto de dados. */
const criteria = defineModel<FilterCriterion[]>({ required: true });
const props = defineProps<{ source: DataSource }>();

const catalog = useCatalogStore();
const fields = computed(() => (catalog.source(props.source)?.fields ?? []).filter((f) => f.type !== 'id'));
const fieldOf = (key: string) => fields.value.find((f) => f.key === key);

const arity = (op: FilterOperator) =>
  op === 'isEmpty' || op === 'isNotEmpty' ? 'none' : op === 'in' || op === 'notIn' ? 'list' : op === 'between' ? 'range' : 'single';

function initialValue(field: DataSourceField | undefined, op: FilterOperator): FilterCriterion['value'] {
  const kind = arity(op);
  if (kind === 'none') return null;
  if (kind === 'list') return field?.options[0] ? [field.options[0].value] : [];
  if (kind === 'range') return field?.type === 'number' ? [0, 0] : ['', ''];
  if (field?.type === 'boolean') return true;
  if (field?.type === 'enum') return field.options[0]?.value ?? '';
  return field?.type === 'number' ? 0 : '';
}

function add() {
  const field = fields.value[0];
  if (!field) return;
  criteria.value = [...criteria.value, { field: field.key, operator: field.operators[0]!, value: initialValue(field, field.operators[0]!) }];
}

function update(index: number, patch: Partial<FilterCriterion>) {
  criteria.value = criteria.value.map((c, i) => {
    if (i !== index) return c;
    const next = { ...c, ...patch };
    if (patch.field && patch.field !== c.field) {
      const field = fieldOf(patch.field);
      next.operator = field?.operators[0] ?? 'eq';
      next.value = initialValue(field, next.operator);
    } else if (patch.operator && arity(patch.operator) !== arity(c.operator)) {
      next.value = initialValue(fieldOf(c.field), patch.operator);
    }
    return next;
  });
}

const remove = (index: number) => (criteria.value = criteria.value.filter((_, i) => i !== index));

const parse = (field: DataSourceField | undefined, raw: string): Scalar =>
  field?.type === 'number' ? (raw === '' ? 0 : Number(raw)) : field?.type === 'boolean' ? raw === 'true' : raw;

function setRange(index: number, position: 0 | 1, raw: string) {
  const c = criteria.value[index]!;
  const range = [...((c.value as Scalar[]) ?? ['', ''])];
  range[position] = parse(fieldOf(c.field), raw);
  update(index, { value: range });
}

function toggleOption(index: number, option: string) {
  const current = (criteria.value[index]!.value as Scalar[]) ?? [];
  update(index, { value: current.includes(option) ? current.filter((v) => v !== option) : [...current, option] });
}

const inputType = (field: DataSourceField | undefined) => (field?.type === 'number' ? 'number' : field?.type === 'date' ? 'date' : 'text');
</script>

<template>
  <div class="stack" style="gap: 8px">
    <div v-for="(c, i) in criteria" :key="i" class="criterion">
      <select class="input input-sm" :value="c.field" :aria-label="`Campo da condição ${i + 1}`" @change="update(i, { field: ($event.target as HTMLSelectElement).value })">
        <option v-for="f in fields" :key="f.key" :value="f.key">{{ f.label }}</option>
      </select>
      <select
        class="input input-sm"
        :value="c.operator"
        :aria-label="`Operador da condição ${i + 1}`"
        @change="update(i, { operator: ($event.target as HTMLSelectElement).value as FilterOperator })"
      >
        <option v-for="op in fieldOf(c.field)?.operators ?? []" :key="op" :value="op">{{ operatorLabel[op] }}</option>
      </select>

      <template v-if="arity(c.operator) === 'single'">
        <select
          v-if="fieldOf(c.field)?.type === 'enum' || fieldOf(c.field)?.type === 'boolean'"
          class="input input-sm"
          :value="String(c.value)"
          :aria-label="`Valor da condição ${i + 1}`"
          @change="update(i, { value: parse(fieldOf(c.field), ($event.target as HTMLSelectElement).value) })"
        >
          <template v-if="fieldOf(c.field)?.type === 'boolean'">
            <option value="true">Sim</option>
            <option value="false">Não</option>
          </template>
          <option v-for="o in fieldOf(c.field)?.options ?? []" :key="o.value" :value="o.value">{{ o.label }}</option>
        </select>
        <input
          v-else
          class="input input-sm"
          :type="inputType(fieldOf(c.field))"
          step="any"
          :value="c.value ?? ''"
          :aria-label="`Valor da condição ${i + 1}`"
          @change="update(i, { value: parse(fieldOf(c.field), ($event.target as HTMLInputElement).value) })"
        />
      </template>

      <div v-else-if="arity(c.operator) === 'range'" class="row" style="gap: 4px">
        <input class="input input-sm" :type="inputType(fieldOf(c.field))" step="any" :value="(c.value as Scalar[])?.[0] ?? ''" aria-label="De" @change="setRange(i, 0, ($event.target as HTMLInputElement).value)" />
        <span class="muted small">e</span>
        <input class="input input-sm" :type="inputType(fieldOf(c.field))" step="any" :value="(c.value as Scalar[])?.[1] ?? ''" aria-label="Até" @change="setRange(i, 1, ($event.target as HTMLInputElement).value)" />
      </div>

      <div v-else-if="arity(c.operator) === 'list'" class="options">
        <template v-if="fieldOf(c.field)?.options.length">
          <label v-for="o in fieldOf(c.field)!.options" :key="o.value" class="small">
            <input type="checkbox" :checked="(c.value as Scalar[])?.includes(o.value)" @change="toggleOption(i, o.value)" /> {{ o.label }}
          </label>
        </template>
        <input
          v-else
          class="input input-sm"
          placeholder="valores separados por vírgula"
          :value="((c.value as Scalar[]) ?? []).join(', ')"
          @change="update(i, { value: ($event.target as HTMLInputElement).value.split(',').map((v) => v.trim()).filter(Boolean) })"
        />
      </div>

      <button class="btn btn-sm" type="button" :aria-label="`Remover condição ${i + 1}`" @click="remove(i)">✕</button>
    </div>

    <p v-if="criteria.length === 0" class="muted small" style="margin: 0">Sem condições: todos os registros.</p>
    <div><button class="btn btn-sm" type="button" @click="add">+ Condição</button></div>
  </div>
</template>

<style scoped>
.criterion {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.options {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 12px;
}

.options label {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}
</style>
