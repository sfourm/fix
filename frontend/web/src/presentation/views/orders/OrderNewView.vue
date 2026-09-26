<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useQueueStore } from '@/application/stores/queue.store';
import { COMMODITIES, type Commodity } from '@/domain/common';
import { commodityLabel, mandateTypeLabel, unitLabel } from '@/domain/labels';
import type { Counterparty } from '@/domain/counterparty';
import type { Mandate } from '@/domain/mandate';
import { ORDER_TYPES, orderTypesFor, type OrderTermsDraft } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';
import { formatNumber, orNull, today } from '../../composables/format';
import OrderTermsForm from '../../components/order/OrderTermsForm.vue';
import PageHeader from '../../components/PageHeader.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';

/**
 * Registro de boleta. Com mandato (cadeia 1:N) a política e o mandato vêm da rota; sem mandato (página de exceções) é um
 * desvio: permitido, mas com justificativa, FORA e na fila de aprovação (FIX2 · I-01). A prévia mostra os desvios que o
 * core vai carimbar (estouro de saldo, tela diferente, venda descoberta) antes de enviar.
 */
const props = defineProps<{ policyId?: string; mandateId?: string }>();

const api = useApi();
const organization = useOrganizationStore();
const queue = useQueueStore();
const router = useRouter();
const toast = useToast();
const trail = useTrailStore();

const withoutMandate = computed(() => !props.mandateId);
const mandate = ref<Mandate | null>(null);
const counterparties = ref<Counterparty[]>([]);
const counterpartyId = ref('');
const commodity = ref<Commodity>('RawSugar');
const justification = ref('');
/** O core recusou por desvio sem justificativa (ex.: saldo consumido por outra boleta enquanto preenchia). */
const serverAskedJustification = ref(false);

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
  coveredSale: false,
});

const types = computed(() => (withoutMandate.value ? [...ORDER_TYPES] : mandate.value ? orderTypesFor(mandate.value.type) : []));
const blocked = computed(() => {
  const m = mandate.value;
  if (withoutMandate.value || !m) return null;
  if (m.status !== 'Active') return 'O mandato não está ativo: só mandatos ativos autorizam boletas. Se a operação já foi feita, registre-a sem mandato em Exceções.';
  if (types.value.length === 0) return `Mandatos de ${mandateTypeLabel[m.type].toLowerCase()} não aceitam boletas de hedge.`;
  return null;
});

/** Prévia do enquadramento (o core refaz e é quem decide). */
const deviations = computed(() => {
  const list: string[] = [];
  const m = mandate.value;
  if (withoutMandate.value) list.push('sem mandato');
  else if (m) {
    const quantity = terms.type === 'Ndf' ? terms.notionalUsd : terms.lots;
    if (m.balance !== null && quantity && quantity > m.balance) list.push(`excede o saldo do ${m.code} (${formatNumber(m.balance)} disponível)`);
    if (m.terms.tenor && terms.tenor && terms.tenor.trim().toUpperCase() !== m.terms.tenor.toUpperCase()) {
      list.push(`tela ${terms.tenor.toUpperCase()} diferente da do ${m.code} (${m.terms.tenor})`);
    }
  }
  if (terms.type === 'Option' && terms.direction === 'Sell' && !terms.coveredSale) list.push('venda descoberta de opção — vedada (§8)');
  return list;
});
const needsJustification = computed(() => deviations.value.length > 0 || serverAskedJustification.value);

onMounted(async () => {
  try {
    const [m, homologated] = await Promise.all([
      props.mandateId ? api.mandates.get(props.mandateId) : Promise.resolve(null),
      organization.can(Permission.ViewCounterparties) ? api.counterparties.list(true) : Promise.resolve([]),
    ]);
    mandate.value = m;
    if (m) {
      trail.set(m.policyId, `${m.policyCode.toUpperCase()} ${m.policyVersion}`);
      trail.set(m.id, `${m.code} · ${m.terms.title}`);
      if (m.terms.tenor) terms.tenor = m.terms.tenor;
      if (types.value.length && !types.value.includes(terms.type)) terms.type = types.value[0]!;
    }
    counterparties.value = homologated;
    counterpartyId.value = homologated[0]?.id ?? '';
  } catch (e) {
    toast.error(errorMessage(e));
  }
});

const submit = useSubmit();
async function register() {
  const order = await submit.run(() =>
    api.orders.register({
      mandateId: props.mandateId ?? null,
      counterpartyId: counterpartyId.value,
      terms: {
        ...terms,
        price: terms.price ?? 0,
        priceUnit: orNull(terms.priceUnit),
        notes: orNull(terms.notes),
        commodity: withoutMandate.value && terms.type !== 'Ndf' ? commodity.value : null,
        justification: needsJustification.value ? orNull(justification.value) : null,
      },
    }),
  );
  if (order) {
    toast.success(
      order.compliance.status === 'Outside'
        ? `Boleta ${order.code} registrada FORA (${order.compliance.reason}): aguardando aprovação.`
        : order.approval === 'Approved'
          ? `Boleta ${order.code} registrada e aprovada (consumiu saldo do mandato).`
          : `Boleta ${order.code} registrada: aguardando aprovação.`,
    );
    queue.refresh();
    router.push(paths.orderOf(order, props.policyId));
  } else if (submit.error.value?.startsWith('Desvio:')) {
    serverAskedJustification.value = true;
  }
}

const cancelTo = computed(() => (props.policyId && props.mandateId ? paths.mandate(props.policyId, props.mandateId) : paths.exceptions()));
</script>

<template>
  <PageHeader
    :kicker="mandate ? `Boleta de hedge · ${mandate.code} · ${mandate.terms.title}` : 'Boleta de hedge · exceção'"
    :title="withoutMandate ? 'Registrar boleta sem mandato' : 'Registrar boleta'"
    :subtitle="
      withoutMandate
        ? 'Operação feita fora de um mandato: fica registrada, FORA, na fila de aprovação e carimbada. Depois, vincule-a a um mandato (a posteriori).'
        : 'A boleta consome o saldo deste mandato quando aprovada e exige contraparte homologada.'
    "
  />

  <div class="layout">
    <form class="card card-body stack" @submit.prevent="register">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <p v-if="blocked" class="alert alert-error">{{ blocked }}</p>
      <p v-if="!organization.can(Permission.SelfApprove)" class="alert alert-info">Você não tem alçada de emissão: a boleta vai para a fila de aprovação.</p>

      <div class="form-grid">
        <div class="field">
          <label for="order-counterparty">Contraparte (homologadas)</label>
          <select id="order-counterparty" v-model="counterpartyId" class="input" required>
            <option v-for="c in counterparties" :key="c.id" :value="c.id">{{ c.code }} · {{ c.name }}</option>
          </select>
          <span v-if="counterparties.length === 0" class="field-error">Nenhuma contraparte homologada: homologue uma em Organização › Contrapartes.</span>
        </div>
        <div v-if="withoutMandate && terms.type !== 'Ndf'" class="field">
          <label for="order-commodity">Commodity</label>
          <select id="order-commodity" v-model="commodity" class="input">
            <option v-for="c in COMMODITIES" :key="c" :value="c">{{ commodityLabel[c] }}</option>
          </select>
        </div>
      </div>

      <OrderTermsForm v-model="terms" :types="types" :field-error="submit.fieldError" />

      <!-- Desvio não bloqueia — expõe: com justificativa a boleta é registrada FORA e vai para aprovação. -->
      <div v-if="needsJustification" class="deviation stack">
        <div>
          <strong>Desvio: a boleta será registrada FORA e irá para aprovação</strong>
          <ul v-if="deviations.length">
            <li v-for="d in deviations" :key="d">{{ d }}</li>
          </ul>
        </div>
        <div class="field">
          <label for="order-justification">Justificativa do desvio</label>
          <textarea id="order-justification" v-model="justification" class="input" maxlength="500" required placeholder="Por que a operação precisa sair assim" />
        </div>
      </div>

      <div class="row">
        <button class="btn btn-primary" type="submit" :disabled="submit.submitting.value || !!blocked || !counterpartyId">
          {{ needsJustification ? 'Registrar com desvio' : 'Registrar boleta' }}
        </button>
        <RouterLink class="btn" :to="cancelTo">Cancelar</RouterLink>
      </div>
    </form>

    <aside class="card rail">
      <template v-if="mandate">
        <h4>Mandato</h4>
        <strong>{{ mandate.code }} · {{ mandate.terms.title }}</strong>
        <div class="delta"><span>Tipo</span><span>{{ mandateTypeLabel[mandate.type] }}</span></div>
        <div class="delta"><span>Eixo</span><span>{{ mandate.axisCode }}</span></div>
        <div class="delta"><span>Tela</span><span class="num">{{ mandate.terms.tenor ?? '—' }}</span></div>
        <div class="delta">
          <span>Saldo</span>
          <span class="num">{{ mandate.balance === null ? 'sem teto' : `${formatNumber(mandate.balance)} ${unitLabel[mandate.terms.quantityUnit]}` }}</span>
        </div>
      </template>
      <h4 :class="{ spaced: mandate }">Enquadramento previsto</h4>
      <span class="badge" :class="deviations.length ? 'badge-danger' : 'badge-success'">{{ deviations.length ? 'FORA' : 'Dentro' }}</span>
      <p class="muted small" style="margin: 8px 0 0">
        {{ deviations.length ? deviations.join(' · ') : 'Sem desvios: com alçada de emissão, entra aprovada e consome o saldo.' }}
      </p>
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
  align-items: flex-start;
  gap: 2px;
  padding: 16px 18px;
}

.rail h4 {
  margin: 0 0 6px;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--primary);
}

.rail h4.spaced {
  margin-top: 14px;
}

.rail strong {
  margin-bottom: 6px;
}

.delta {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  width: 100%;
  padding: 7px 0;
  border-bottom: 1px solid var(--border);
  font-size: 0.86rem;
}

.delta span:first-child {
  color: var(--text-muted);
}

.deviation {
  padding: 14px 16px;
  border-radius: 14px;
  background: var(--danger-soft);
  color: var(--text);
}

.deviation ul {
  margin: 6px 0 0;
  padding-left: 18px;
  color: var(--danger);
  font-size: 0.9rem;
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
