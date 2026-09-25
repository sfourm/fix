<script setup lang="ts">
import { reactive, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import type { Policy, PolicyLimits } from '@/domain/policy';
import { useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';

const props = defineProps<{ policy: Policy; editable: boolean }>();
const emit = defineEmits<{ updated: [policy: Policy] }>();

const api = useApi();
const toast = useToast();
const form = reactive<PolicyLimits>({ ...props.policy.limits });
const submit = useSubmit();

watch(
  () => props.policy.limits,
  (limits) => Object.assign(form, limits),
);

/** Parâmetros agrupados como nas seções da política (§). */
const groups: { title: string; fields: { key: keyof PolicyLimits; label: string; suffix: string }[] }[] = [
  {
    title: 'Cobertura e horizonte',
    fields: [
      { key: 'hedgeHorizonYears', label: 'Horizonte máximo de hedge · §7.1', suffix: 'anos' },
      { key: 'absoluteCeilingPct', label: 'Teto absoluto de cobertura sobre o disponível · §6.3', suffix: '%' },
      { key: 'coveredCallMaxPct', label: 'Venda coberta de opções · §8', suffix: '%' },
    ],
  },
  {
    title: 'Câmbio · §6.2',
    fields: [
      { key: 'fxFixedMinPct', label: 'NDF mínimo sobre a receita fixada', suffix: '%' },
      { key: 'fxFixedMaxPct', label: 'NDF máximo sobre a receita fixada', suffix: '%' },
      { key: 'fxUnfixedMaxPct', label: 'Proteção antecipada sobre a receita não fixada', suffix: '%' },
    ],
  },
  {
    title: 'Liquidez e concentração · §6.4–6.5',
    fields: [
      { key: 'marginCashMaxPct', label: 'Margem em corretoras sobre o caixa', suffix: '%' },
      { key: 'physicalConcentrationMaxPct', label: 'Concentração por contraparte · físico', suffix: '%' },
      { key: 'financialConcentrationMaxPct', label: 'Concentração por contraparte · financeiro OTC', suffix: '%' },
    ],
  },
  {
    title: 'Logística · §5.4 e §7.2',
    fields: [
      { key: 'logisticsDeadlineMonths', label: 'Prazo-limite para contratar o frete', suffix: 'meses' },
      { key: 'freightCeilingPct', label: 'Tarifa máxima sobre a referência de mercado', suffix: '%' },
    ],
  },
];

async function save() {
  const updated = await submit.run(() => api.policies.updateLimits(props.policy.id, { ...form }));
  if (updated) {
    emit('updated', updated);
    toast.success('Parâmetros salvos.');
  }
}
</script>

<template>
  <form class="card-body stack" @submit.prevent="save">
    <p v-if="!editable" class="alert alert-info">Política vigente é somente leitura: abra uma nova versão para alterar os parâmetros.</p>
    <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>

    <template v-for="group in groups" :key="group.title">
      <h3 class="section-title">{{ group.title }}</h3>
      <div class="form-grid">
        <div v-for="field in group.fields" :key="field.key" class="field">
          <label :for="`limit-${field.key}`">{{ field.label }} ({{ field.suffix }})</label>
          <input
            :id="`limit-${field.key}`"
            v-model.number="form[field.key]"
            class="input"
            :class="{ invalid: submit.fieldError(field.key) }"
            type="number"
            step="any"
            min="0"
            required
            :disabled="!editable"
          />
          <span v-if="submit.fieldError(field.key)" class="field-error">{{ submit.fieldError(field.key) }}</span>
        </div>
      </div>
    </template>

    <div v-if="editable"><button class="btn btn-primary" type="submit" :disabled="submit.submitting.value">Salvar parâmetros</button></div>
  </form>
</template>
