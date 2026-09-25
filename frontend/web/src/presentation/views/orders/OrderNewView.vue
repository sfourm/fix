<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { mandateTypeLabel, unitLabel } from '@/domain/labels';
import type { Counterparty } from '@/domain/counterparty';
import type { Mandate } from '@/domain/mandate';
import { orderTypesFor, type OrderTermsDraft } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';
import { formatNumber, orNull, today } from '../../composables/format';
import OrderTermsForm from '../../components/order/OrderTermsForm.vue';
import PageHeader from '../../components/PageHeader.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';

/** Registro de boleta dentro de um mandato (cadeia 1:N): política e mandato vêm da rota. */
const props = defineProps<{ policyId: string; mandateId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const toast = useToast();
const trail = useTrailStore();

const mandate = ref<Mandate | null>(null);
const counterparties = ref<Counterparty[]>([]);
const counterpartyId = ref('');

const terms = reactive<OrderTermsDraft>({
  type: 'Futures',
  direction: 'Sell',
  tenor: '',
  lots: null,
  notionalUsd: null,
  price: null,
  priceUnit: 'c/lb',
  optionKind: null,
  premium: null,
  tradeDate: today(),
  notes: null,
});

const types = computed(() => (mandate.value ? orderTypesFor(mandate.value.type) : []));
const blocked = computed(() => {
  const m = mandate.value;
  if (!m) return null;
  if (m.status !== 'Active') return 'O mandato não está ativo: só mandatos ativos aceitam boletas.';
  if (types.value.length === 0) return `Mandatos de ${mandateTypeLabel[m.type].toLowerCase()} não aceitam boletas de hedge.`;
  return null;
});

onMounted(async () => {
  try {
    const [m, homologated] = await Promise.all([
      api.mandates.get(props.mandateId),
      organization.can(Permission.ViewCounterparties) ? api.counterparties.list(true) : Promise.resolve([]),
    ]);
    mandate.value = m;
    trail.set(m.policyId, `${m.policyCode.toUpperCase()} ${m.policyVersion}`);
    trail.set(m.id, m.terms.title);
    counterparties.value = homologated;
    counterpartyId.value = homologated[0]?.id ?? '';
    if (m.terms.tenor) terms.tenor = m.terms.tenor;
    if (types.value.length && !types.value.includes(terms.type)) terms.type = types.value[0]!;
  } catch (e) {
    toast.error(errorMessage(e));
  }
});

const submit = useSubmit();
async function register() {
  const order = await submit.run(() =>
    api.orders.register({
      mandateId: props.mandateId,
      counterpartyId: counterpartyId.value,
      terms: { ...terms, price: terms.price ?? 0, priceUnit: orNull(terms.priceUnit), notes: orNull(terms.notes) },
    }),
  );
  if (order) {
    toast.success(order.approval === 'Approved' ? 'Boleta registrada e aprovada (consumiu saldo do mandato).' : 'Boleta registrada: aguardando aprovação.');
    queue.refresh();
    router.push(paths.order(props.policyId, props.mandateId, order.id));
  }
}
</script>

<template>
  <PageHeader
    :kicker="mandate ? `Boleta de hedge · mandato ${mandate.terms.title}` : 'Boleta de hedge'"
    title="Registrar boleta"
    subtitle="A boleta consome o saldo deste mandato quando aprovada e exige contraparte homologada."
  />

  <div class="layout">
    <form class="card card-body stack" @submit.prevent="register">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <p v-if="blocked" class="alert alert-error">{{ blocked }}</p>
      <p v-if="!organization.can(Permission.SelfApprove)" class="alert alert-info">Você não tem alçada de emissão: a boleta vai para a fila de aprovação da política.</p>

      <div class="field">
        <label for="order-counterparty">Contraparte (homologadas)</label>
        <select id="order-counterparty" v-model="counterpartyId" class="input" required>
          <option v-for="c in counterparties" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
        <span v-if="counterparties.length === 0" class="field-error">Nenhuma contraparte homologada: homologue uma em Organização › Contrapartes.</span>
      </div>

      <OrderTermsForm v-model="terms" :types="types" :field-error="submit.fieldError" />

      <div class="row">
        <button class="btn btn-primary" type="submit" :disabled="submit.submitting.value || !!blocked || !counterpartyId">Registrar boleta</button>
        <RouterLink class="btn" :to="paths.mandate(props.policyId, props.mandateId)">Cancelar</RouterLink>
      </div>
    </form>

    <aside v-if="mandate" class="card rail">
      <h4>Mandato</h4>
      <strong>{{ mandate.terms.title }}</strong>
      <div class="delta"><span>Tipo</span><span>{{ mandateTypeLabel[mandate.type] }}</span></div>
      <div class="delta"><span>Eixo</span><span>{{ mandate.axisCode }}</span></div>
      <div class="delta"><span>Tela</span><span class="num">{{ mandate.terms.tenor ?? '—' }}</span></div>
      <div class="delta">
        <span>Saldo</span>
        <span class="num">{{ mandate.balance === null ? 'sem teto' : `${formatNumber(mandate.balance)} ${unitLabel[mandate.terms.quantityUnit]}` }}</span>
      </div>
      <p class="muted small" style="margin: 8px 0 0">Só boletas aprovadas consomem saldo.</p>
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

.rail {
  position: sticky;
  top: calc(var(--topbar-h) + 16px);
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 16px 18px;
}

.rail h4 {
  margin: 0 0 6px;
  font-size: 0.66rem;
  font-weight: 600;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: var(--primary);
}

.rail strong {
  margin-bottom: 6px;
}

.delta {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  padding: 7px 0;
  border-bottom: 1px solid var(--border);
  font-size: 0.86rem;
}

.delta span:first-child {
  color: var(--text-muted);
}

@media (max-width: 1000px) {
  .layout {
    grid-template-columns: 1fr;
  }

  .rail {
    position: static;
  }
}
</style>
