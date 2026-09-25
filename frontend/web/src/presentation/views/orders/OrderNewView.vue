<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { unitLabel } from '@/domain/labels';
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

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const route = useRoute();
const router = useRouter();
const toast = useToast();

const mandates = ref<Mandate[]>([]);
const counterparties = ref<Counterparty[]>([]);
const mandateId = ref((route.query.mandateId as string | undefined) ?? '');
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

const mandate = computed(() => mandates.value.find((m) => m.id === mandateId.value) ?? null);
const types = computed(() => (mandate.value ? orderTypesFor(mandate.value.type) : []));

onMounted(async () => {
  try {
    const [active, homologated] = await Promise.all([
      api.mandates.list({ status: 'Active', pageSize: 100 }),
      organization.can(Permission.ViewCounterparties) ? api.counterparties.list(true) : Promise.resolve([]),
    ]);
    mandates.value = active.items.filter((m) => orderTypesFor(m.type).length > 0);
    counterparties.value = homologated;
    mandateId.value ||= mandates.value[0]?.id ?? '';
    counterpartyId.value = counterparties.value[0]?.id ?? '';
    if (mandate.value?.terms.tenor) terms.tenor = mandate.value.terms.tenor;
  } catch (e) {
    toast.error(errorMessage(e));
  }
});

function selectMandate() {
  if (mandate.value?.terms.tenor) terms.tenor = mandate.value.terms.tenor;
}

const submit = useSubmit();
async function register() {
  const order = await submit.run(() =>
    api.orders.register({
      mandateId: mandateId.value,
      counterpartyId: counterpartyId.value,
      terms: { ...terms, price: terms.price ?? 0, priceUnit: orNull(terms.priceUnit), notes: orNull(terms.notes) },
    }),
  );
  if (order) {
    toast.success(order.approval === 'Approved' ? 'Boleta registrada e aprovada (consumiu saldo do mandato).' : 'Boleta registrada: aguardando aprovação.');
    queue.refresh();
    router.push(`/orders/${order.id}`);
  }
}
</script>

<template>
  <PageHeader title="Nova boleta de hedge" subtitle="A boleta consome um mandato ativo e exige contraparte homologada.">
    <template #breadcrumb>
      <nav class="breadcrumb"><RouterLink to="/orders">Boletas de hedge</RouterLink><span>›</span><span>nova</span></nav>
    </template>
  </PageHeader>

  <form class="card card-body stack" @submit.prevent="register">
    <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
    <p v-if="mandates.length === 0" class="alert alert-info">Nenhum mandato ativo de precificação ou moeda. Emita e aprove um mandato antes de registrar boletas.</p>
    <p v-if="!organization.can(Permission.SelfApprove)" class="alert alert-info">Você não tem alçada de emissão: a boleta vai para a fila de aprovação.</p>

    <div class="form-grid">
      <div class="field">
        <label for="order-mandate">Mandato</label>
        <select id="order-mandate" v-model="mandateId" class="input" required @change="selectMandate">
          <option v-for="m in mandates" :key="m.id" :value="m.id">{{ m.terms.title }} · {{ m.axisCode }}</option>
        </select>
        <span v-if="mandate" class="muted small">
          Saldo: {{ mandate.balance === null ? 'sem teto' : `${formatNumber(mandate.balance)} ${unitLabel[mandate.terms.quantityUnit]}` }}
        </span>
      </div>
      <div class="field">
        <label for="order-counterparty">Contraparte (homologadas)</label>
        <select id="order-counterparty" v-model="counterpartyId" class="input" required>
          <option v-for="c in counterparties" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
      </div>
    </div>

    <OrderTermsForm v-model="terms" :types="types" :field-error="submit.fieldError" />

    <div class="row">
      <button class="btn btn-primary" type="submit" :disabled="submit.submitting.value || !mandateId || !counterpartyId">Registrar boleta</button>
      <RouterLink class="btn" to="/orders">Cancelar</RouterLink>
    </div>
  </form>
</template>
