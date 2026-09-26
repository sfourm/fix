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
  {
    title: 'Contingência escalonada · reserva que nunca é vendida nem fixada',
    fields: [
      { key: 'contingency1MonthPct', label: 'Produção a até 1 mês', suffix: '%' },
      { key: 'contingency6MonthsPct', label: 'Produção a até 6 meses', suffix: '%' },
      { key: 'contingency12MonthsPct', label: 'Produção a até 12 meses', suffix: '%' },
      { key: 'contingency24MonthsPct', label: 'Produção a até 24 meses', suffix: '%' },
      { key: 'contingency36MonthsPct', label: 'Produção a 36 meses ou mais', suffix: '%' },
    ],
  },
  {
    title: 'Recompra e estresse · §11',
    fields: [
      { key: 'buybackTriggerPct', label: 'Recompra quando o vendido passa do novo disponível', suffix: '%' },
      { key: 'buybackDeadlineBusinessDays', label: 'Prazo da recompra', suffix: 'dias úteis' },
      { key: 'stressSigmas', label: 'Choque de estresse de caixa', suffix: 'σ' },
      { key: 'stressDays', label: 'Horizonte do estresse', suffix: 'dias úteis' },
    ],
  },
  {
    title: 'Régua de fixação e mix',
    fields: [
      { key: 'pricingHotPercentile', label: 'Mercado "quente" a partir do percentil (FG/A)', suffix: 'p' },
      { key: 'pricingColdPercentile', label: 'Mercado "frio" até o percentil (FG/A)', suffix: 'p' },
      { key: 'mixShiftMaxPp', label: 'Virada de mix que exige rito', suffix: 'p.p.' },
    ],
  },
  {
    title: 'Controles',
    fields: [
      { key: 'confirmationDeadlineBusinessDays', label: 'Prazo da confirmação da contraparte', suffix: 'dias úteis' },
      { key: 'registrationDeadlineDays', label: 'Registro da boleta após a execução (0 = D+0)', suffix: 'dias' },
      { key: 'deviationReportHours', label: 'Reporte de desvio', suffix: 'horas' },
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
