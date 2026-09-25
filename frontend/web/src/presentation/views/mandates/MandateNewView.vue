<script setup lang="ts">
import { computed, onMounted, reactive, ref, shallowRef, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { complianceLabel, complianceTone, mandateTypeLabel, policyStatusLabel, riskFactorLabel } from '@/domain/labels';
import { MANDATE_TYPES, mandateTypeFactor, type Compliance, type MandateIssueInput, type MandateTerms, type MandateType } from '@/domain/mandate';
import { Permission } from '@/domain/permissions';
import type { Policy } from '@/domain/policy';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';
import { orNull } from '../../composables/format';
import MandateTermsForm from '../../components/mandate/MandateTermsForm.vue';
import PageHeader from '../../components/PageHeader.vue';
import StatusBadge from '../../components/StatusBadge.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';

/** Emissão de mandato dentro de uma política (cadeia 1:N): a política vem da rota. */
const props = defineProps<{ policyId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const trail = useTrailStore();
const toast = useToast();

const policyId = computed(() => props.policyId);
const policy = shallowRef<Policy | null>(null);
const type = ref<MandateType>('Pricing');
const axisId = ref('');

const terms = reactive<MandateTerms>({
  title: '',
  criteria: null,
  commodity: organization.current?.commodities[0]?.commodity ?? 'RawSugar',
  tenor: null,
  quantity: null,
  quantityUnit: 'Lots',
  price: { atMarket: false, target: null, min: null, max: null, unit: 'c/lb' },
  windowStart: null,
  windowEnd: null,
});

const axes = computed(() => policy.value?.axes.filter((a) => a.factor === mandateTypeFactor[type.value]) ?? []);
const selfApprove = computed(() => organization.can(Permission.SelfApprove));

onMounted(async () => {
  try {
    policy.value = await api.policies.get(props.policyId);
    trail.set(policy.value.id, `${policy.value.code.toUpperCase()} ${policy.value.version}`);
  } catch (e) {
    toast.error(errorMessage(e));
  }
});

// Troca de política ou tipo: seleciona o primeiro eixo compatível e ajusta a unidade padrão.
watch([axes, type], () => {
  if (!axes.value.some((a) => a.id === axisId.value)) axisId.value = axes.value[0]?.id ?? '';
});
watch(type, (value) => {
  terms.quantityUnit = value === 'Currency' ? 'Usd' : value === 'Pricing' ? 'Lots' : 'Tonnes';
  terms.price.unit = value === 'Currency' ? 'R$/US$' : value === 'Pricing' ? 'c/lb' : 'R$/t';
});

function input(): MandateIssueInput {
  const atMarket = terms.price.atMarket;
  return {
    policyId: policyId.value,
    axisId: axisId.value,
    type: type.value,
    terms: {
      ...terms,
      criteria: orNull(terms.criteria),
      tenor: orNull(terms.tenor),
      windowStart: orNull(terms.windowStart),
      windowEnd: orNull(terms.windowEnd),
      price: {
        atMarket,
        target: atMarket ? null : terms.price.target,
        min: atMarket ? null : terms.price.min,
        max: atMarket ? null : terms.price.max,
        unit: orNull(terms.price.unit),
      },
    },
  };
}

// ---------- Prévia de enquadramento (não grava nada) ----------
const compliance = ref<Compliance | null>(null);
const previewError = ref<string | null>(null);
let previewTimer: ReturnType<typeof setTimeout> | undefined;

watch(
  [policyId, axisId, type, () => JSON.stringify(terms)],
  () => {
    clearTimeout(previewTimer);
    if (!policyId.value || !axisId.value || !terms.title.trim()) {
      compliance.value = null;
      return;
    }
    previewTimer = setTimeout(async () => {
      try {
        compliance.value = await api.mandates.preview(input());
        previewError.value = null;
      } catch (e) {
        compliance.value = null;
        previewError.value = errorMessage(e);
      }
    }, 400);
  },
  { deep: true },
);

const outcome = computed(() => {
  if (!compliance.value) return null;
  if (compliance.value.status === 'Outside') return 'Fora da política: vai para a fila e só é aprovado por quem tem alçada de exceção.';
  return selfApprove.value ? 'Dentro da política e você tem alçada de emissão: o mandato entra ativo.' : 'Dentro da política: vai para a fila de aprovação.';
});

const submit = useSubmit();
async function issue() {
  const mandate = await submit.run(() => api.mandates.issue(input()));
  if (mandate) {
    toast.success(mandate.status === 'Active' ? 'Mandato emitido e ativo.' : 'Mandato emitido: aguardando aprovação.');
    queue.refresh();
    router.push(paths.mandate(props.policyId, mandate.id));
  }
}
</script>

<template>
  <PageHeader
    :kicker="policy ? `Mandato · ${policy.code} ${policy.version} · ${policyStatusLabel[policy.status]}` : 'Mandato'"
    title="Emitir mandato"
    subtitle="Escolha o tipo e o eixo desta política, descreva o volume e o preço: o enquadramento é checado antes de emitir."
  />

  <div class="layout">
    <form class="card card-body stack" @submit.prevent="issue">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>

      <p v-if="policy && policy.status !== 'Active'" class="alert alert-info">A política não está vigente: a emissão será recusada.</p>
      <div class="form-grid">
        <div class="field">
          <label for="mandate-type">Tipo</label>
          <select id="mandate-type" v-model="type" class="input">
            <option v-for="t in MANDATE_TYPES" :key="t" :value="t">{{ mandateTypeLabel[t] }}</option>
          </select>
        </div>
        <div class="field">
          <label for="mandate-axis">Eixo da política ({{ riskFactorLabel[mandateTypeFactor[type]] }})</label>
          <select id="mandate-axis" v-model="axisId" class="input" required>
            <option v-for="a in axes" :key="a.id" :value="a.id">{{ a.code }} · {{ a.title }}</option>
          </select>
          <span v-if="policy && axes.length === 0" class="field-error">A política não tem eixo de {{ riskFactorLabel[mandateTypeFactor[type]] }}.</span>
        </div>
      </div>

      <MandateTermsForm v-model="terms" :type="type" :field-error="submit.fieldError" />

      <div class="row">
        <button class="btn btn-primary" type="submit" :disabled="submit.submitting.value || !axisId">Emitir mandato</button>
        <RouterLink class="btn" :to="paths.policy(props.policyId, 'mandates')">Cancelar</RouterLink>
      </div>
    </form>

    <aside class="card preview">
      <header class="card-header"><h2>Enquadramento</h2></header>
      <div class="card-body stack">
        <template v-if="compliance">
          <StatusBadge :label="complianceLabel[compliance.status]" :tone="complianceTone[compliance.status]" />
          <p style="margin: 0">{{ compliance.reason }}</p>
          <p class="muted small" style="margin: 0">{{ outcome }}</p>
        </template>
        <p v-else-if="previewError" class="alert alert-error">{{ previewError }}</p>
        <p v-else class="muted small" style="margin: 0">Preencha eixo e título para ver a checagem: vigência, horizonte de hedge, janela e piso econômico.</p>
      </div>
    </aside>
  </div>
</template>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 300px;
  gap: 16px;
  align-items: start;
}

.preview {
  position: sticky;
  top: 16px;
}

@media (max-width: 1000px) {
  .layout {
    grid-template-columns: 1fr;
  }

  .preview {
    position: static;
  }
}
</style>
