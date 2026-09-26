<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { COMMODITIES, MEASUREMENT_UNITS, type Commodity } from '@/domain/common';
import { commodityLabel, sectorLabel, unitLabel } from '@/domain/labels';
import { SECTORS, type OrganizationCommodity, type OrganizationSetup } from '@/domain/organization';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import type { CommodityInput } from '@/infrastructure/api/organization.api';
import { useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatNumber, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import NumberInput from '../../components/NumberInput.vue';
import PageHeader from '../../components/PageHeader.vue';

const api = useApi();
const organization = useOrganizationStore();
const toast = useToast();
const { confirm } = useConfirm();
const route = useRoute();
const router = useRouter();

const canEdit = computed(() => organization.can(Permission.EditOrganization));
const setup = computed(() => organization.current as OrganizationSetup);
const months = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

// Cada aba tem seu formulário, inicializado a partir do setup atual.
const identity = reactive({ name: setup.value.name, ...setup.value.profile });
const industrial = reactive({ ...setup.value.industrial });
const budget = reactive({ ...setup.value.budget });
const financials = reactive({ ...setup.value.financials });

const identitySubmit = useSubmit();
const industrialSubmit = useSubmit();
const budgetSubmit = useSubmit();
const financialsSubmit = useSubmit();

// ---------- Abas ----------
type Tab = 'identity' | 'industrial' | 'budget' | 'commodities' | 'financials';

/** Formulário diferente do que está salvo (ponto de "não salvo" na aba). */
const differs = (form: object, saved: object) =>
  Object.entries(form).some(([key, value]) => key !== 'leverage' && (saved as Record<string, unknown>)[key] !== value);

const tabs = computed<{ key: Tab; label: string; dirty?: boolean; count?: number }[]>(() => [
  { key: 'identity', label: 'Identificação', dirty: differs(identity, { name: setup.value.name, ...setup.value.profile }) },
  { key: 'industrial', label: 'Capacidade industrial', dirty: differs(industrial, setup.value.industrial) },
  { key: 'budget', label: 'Orçamento e gatilhos', dirty: differs(budget, setup.value.budget) },
  { key: 'commodities', label: 'Commodities', count: setup.value.commodities.length },
  { key: 'financials', label: 'Dados financeiros', dirty: differs(financials, setup.value.financials) },
]);

/** A aba aberta fica na URL (?tab=), para voltar direto nela ou compartilhar o link. */
const tab = computed<Tab>(() => (tabs.value.some((t) => t.key === route.query.tab) ? (route.query.tab as Tab) : 'identity'));
const selectTab = (key: Tab) => router.replace({ query: { ...route.query, tab: key === 'identity' ? undefined : key } });

async function save(submit: ReturnType<typeof useSubmit>, action: () => Promise<OrganizationSetup>, message: string) {
  const updated = await submit.run(action);
  if (updated) {
    organization.applySetup(updated);
    toast.success(message);
  }
}

async function saveIdentity() {
  const { name, ...profile } = identity;
  await save(
    identitySubmit,
    async () => {
      if (name !== setup.value.name) await organization.rename(name);
      return api.organizations.updateProfile({
        ...profile,
        taxId: orNull(profile.taxId),
        headquarters: orNull(profile.headquarters),
        group: orNull(profile.group),
        activeCrop: orNull(profile.activeCrop),
      });
    },
    'Identificação salva.',
  );
}

const saveIndustrial = () => save(industrialSubmit, () => api.organizations.updateIndustrial({ ...industrial }), 'Capacidade industrial salva.');
const saveBudget = () => save(budgetSubmit, () => api.organizations.updateBudget({ ...budget }), 'Orçamento salvo.');
const saveFinancials = () =>
  save(
    financialsSubmit,
    async () => {
      const { leverage: _, ...input } = financials;
      const updated = await api.organizations.updateFinancials({ ...input, referenceDate: orNull(input.referenceDate) });
      financials.leverage = updated.financials.leverage;
      return updated;
    },
    'Dados financeiros salvos.',
  );

// ---------- Commodities ----------
const commodityModal = ref<{ editing: OrganizationCommodity | null } | null>(null);
const commodityForm = reactive<Omit<CommodityInput, 'capacity'> & { capacity: number | null; commodity: Commodity }>({
  commodity: 'RawSugar',
  capacity: 0,
  unit: 'Tonnes',
  priceReference: null,
  currency: 'USD',
  sells: true,
});
const commoditySubmit = useSubmit();
const availableCommodities = computed(() =>
  COMMODITIES.filter((c) => c === commodityModal.value?.editing?.commodity || !setup.value.commodities.some((x) => x.commodity === c)),
);

function openCommodity(editing: OrganizationCommodity | null) {
  Object.assign(
    commodityForm,
    editing ?? { commodity: availableCommodities.value[0] ?? 'RawSugar', capacity: 0, unit: 'Tonnes', priceReference: null, currency: 'USD', sells: true },
  );
  commoditySubmit.reset();
  commodityModal.value = { editing };
}

async function saveCommodity() {
  const editing = commodityModal.value?.editing;
  const { commodity, ...input } = commodityForm;
  const data = { ...input, capacity: input.capacity ?? 0, priceReference: orNull(input.priceReference) };
  const updated = await commoditySubmit.run(() =>
    editing ? api.organizations.updateCommodity(editing.id, data) : api.organizations.addCommodity({ commodity, ...data }),
  );
  if (updated) {
    organization.applySetup(updated);
    commodityModal.value = null;
    toast.success(editing ? 'Commodity atualizada.' : 'Commodity adicionada.');
  }
}

async function removeCommodity(item: OrganizationCommodity) {
  const ok = await confirm({
    title: 'Remover commodity',
    message: `Remover ${commodityLabel[item.commodity]} do setup?`,
    confirmLabel: 'Remover',
    danger: true,
  });
  if (!ok) return;
  try {
    organization.applySetup(await api.organizations.removeCommodity(item.id));
    toast.success('Commodity removida.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}
</script>

<template>
  <PageHeader title="Setup da companhia" subtitle="Identificação, capacidade industrial, orçamento e commodities: a base que a política e os mandatos usam para enquadrar as operações." />

  <p v-if="!canEdit" class="alert alert-info">Somente leitura: editar o setup exige a regra edit_organization.</p>

  <nav class="tabs setup-tabs" role="tablist" aria-label="Seções do setup">
    <button
      v-for="t in tabs"
      :key="t.key"
      type="button"
      role="tab"
      class="tab"
      :class="{ active: tab === t.key }"
      :aria-selected="tab === t.key"
      @click="selectTab(t.key)"
    >
      {{ t.label }}
      <span v-if="t.count !== undefined" class="count">{{ t.count }}</span>
      <span v-if="t.dirty" class="dirty" title="Alterações não salvas" aria-label="alterações não salvas" />
    </button>
  </nav>

  <!-- Identificação -->
  <section v-if="tab === 'identity'" class="card">
    <header class="card-header">
      <div>
        <h2>Identificação</h2>
        <p class="muted small hint">Quem é a companhia e qual safra está em curso.</p>
      </div>
    </header>
    <form class="card-body stack" @submit.prevent="saveIdentity">
      <p v-if="identitySubmit.error.value" class="alert alert-error">{{ identitySubmit.error.value }}</p>
      <div class="form-grid">
        <div class="field">
          <label for="org-name">Nome da organização</label>
          <input id="org-name" v-model="identity.name" class="input" maxlength="150" required :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="corporate-name">Razão social</label>
          <input id="corporate-name" v-model="identity.corporateName" class="input" maxlength="200" required :disabled="!canEdit" />
        </div>
      </div>
      <div class="form-grid wide">
        <div class="field">
          <label for="tax-id">CNPJ</label>
          <input id="tax-id" v-model="identity.taxId" class="input" maxlength="32" :disabled="!canEdit" />
          <span v-if="identitySubmit.fieldError('taxId')" class="field-error">{{ identitySubmit.fieldError('taxId') }}</span>
        </div>
        <div class="field">
          <label for="sector">Setor</label>
          <select id="sector" v-model="identity.sector" class="input" :disabled="!canEdit">
            <option v-for="s in SECTORS" :key="s" :value="s">{{ sectorLabel[s] }}</option>
          </select>
        </div>
        <div class="field">
          <label for="headquarters">Sede</label>
          <input id="headquarters" v-model="identity.headquarters" class="input" maxlength="200" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="group">Grupo econômico</label>
          <input id="group" v-model="identity.group" class="input" maxlength="200" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="crop-start">Início do ano-safra</label>
          <select id="crop-start" v-model.number="identity.cropYearStartMonth" class="input" :disabled="!canEdit">
            <option v-for="(m, i) in months" :key="m" :value="i + 1">{{ m }}</option>
          </select>
        </div>
        <div class="field">
          <label for="active-crop">Safra ativa (AA/AA)</label>
          <input id="active-crop" v-model="identity.activeCrop" class="input" placeholder="26/27" maxlength="5" :disabled="!canEdit" />
          <span v-if="identitySubmit.fieldError('activeCrop')" class="field-error">{{ identitySubmit.fieldError('activeCrop') }}</span>
        </div>
      </div>
      <footer v-if="canEdit" class="form-foot">
        <button class="btn btn-primary" type="submit" :disabled="identitySubmit.submitting.value">Salvar identificação</button>
      </footer>
    </form>
  </section>

  <!-- Capacidade industrial -->
  <section v-else-if="tab === 'industrial'" class="card">
    <header class="card-header">
      <div>
        <h2>Capacidade industrial</h2>
        <p class="muted small hint">Moagem e mix de açúcar: limitam o volume que os mandatos podem comprometer.</p>
      </div>
    </header>
    <form class="card-body stack" @submit.prevent="saveIndustrial">
      <p v-if="industrialSubmit.error.value" class="alert alert-error">{{ industrialSubmit.error.value }}</p>
      <div class="form-grid wide">
        <div class="field">
          <label for="milling">Moagem (t/safra)</label>
          <NumberInput id="milling" v-model="industrial.millingCapacity" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="mix-guidance">Mix açúcar orientado (%)</label>
          <NumberInput id="mix-guidance" v-model="industrial.mixGuidancePct" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="mix-min">Mix mínimo (%)</label>
          <NumberInput id="mix-min" v-model="industrial.mixMinPct" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="mix-max">Mix máximo (%)</label>
          <NumberInput id="mix-max" v-model="industrial.mixMaxPct" :disabled="!canEdit" />
          <span v-if="industrialSubmit.fieldError('mixMaxPct')" class="field-error">{{ industrialSubmit.fieldError('mixMaxPct') }}</span>
        </div>
      </div>
      <footer v-if="canEdit" class="form-foot">
        <button class="btn btn-primary" type="submit" :disabled="industrialSubmit.submitting.value">Salvar capacidade</button>
      </footer>
    </form>
  </section>

  <!-- Orçamento e gatilhos -->
  <section v-else-if="tab === 'budget'" class="card">
    <header class="card-header">
      <div>
        <h2>Orçamento e gatilhos</h2>
        <p class="muted small hint">
          Valores em ¢/lb-equivalente. Mandatos de fixação com preço mínimo abaixo do piso ficam fora da política e exigem aprovação de exceção.
        </p>
      </div>
    </header>
    <form class="card-body stack" @submit.prevent="saveBudget">
      <p v-if="budgetSubmit.error.value" class="alert alert-error">{{ budgetSubmit.error.value }}</p>
      <div class="form-grid wide">
        <div class="field">
          <label for="cash-cost">Custo caixa (gatilho de fixação)</label>
          <NumberInput id="cash-cost" v-model="budget.cashCost" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="economic-floor">Piso econômico</label>
          <NumberInput id="economic-floor" v-model="budget.economicFloor" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="equivalent-price">Preço equivalente orçado</label>
          <NumberInput id="equivalent-price" v-model="budget.equivalentPrice" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label for="target-margin">Margem-alvo (%)</label>
          <NumberInput id="target-margin" v-model="budget.targetMarginPct" :disabled="!canEdit" />
        </div>
      </div>
      <footer v-if="canEdit" class="form-foot">
        <button class="btn btn-primary" type="submit" :disabled="budgetSubmit.submitting.value">Salvar orçamento</button>
      </footer>
    </form>
  </section>

  <!-- Commodities -->
  <section v-else-if="tab === 'commodities'" class="card">
    <header class="card-header">
      <div>
        <h2>Commodities</h2>
        <p class="muted small hint">O que a companhia produz e negocia, com capacidade e referência de preço.</p>
      </div>
      <button v-if="canEdit && availableCommodities.length" class="btn btn-primary btn-sm" @click="openCommodity(null)">+ Adicionar commodity</button>
    </header>
    <div v-if="setup.commodities.length" class="table-wrap">
      <table v-columns="'setup-commodities'" class="table">
        <thead>
          <tr><th>Commodity</th><th>Capacidade</th><th>Referência de preço</th><th>Moeda</th><th>Vende</th><th /></tr>
        </thead>
        <tbody>
          <tr v-for="item in setup.commodities" :key="item.id">
            <td><strong>{{ commodityLabel[item.commodity] }}</strong></td>
            <td>{{ formatNumber(item.capacity) }} {{ unitLabel[item.unit] }}</td>
            <td>{{ item.priceReference ?? '—' }}</td>
            <td>{{ item.currency }}</td>
            <td>{{ item.sells ? 'sim' : 'não' }}</td>
            <td class="actions">
              <template v-if="canEdit">
                <button class="btn btn-sm" @click="openCommodity(item)">Editar</button>
                <button class="btn btn-sm btn-danger" @click="removeCommodity(item)">Remover</button>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <p v-else class="card-body muted" style="margin: 0">Nenhuma commodity cadastrada.</p>
  </section>

  <!-- Dados financeiros -->
  <section v-else class="card">
    <header class="card-header">
      <div>
        <h2>Dados financeiros</h2>
        <p class="muted small hint">Alimentam o teto de margem em corretoras e a alavancagem.</p>
      </div>
    </header>
    <form class="card-body stack" @submit.prevent="saveFinancials">
      <p v-if="financialsSubmit.error.value" class="alert alert-error">{{ financialsSubmit.error.value }}</p>
      <div class="form-grid wide">
        <div class="field"><label for="cash">Caixa</label><NumberInput id="cash" v-model="financials.cash" :disabled="!canEdit" /></div>
        <div class="field"><label for="credit">Linhas de crédito</label><NumberInput id="credit" v-model="financials.creditLines" :disabled="!canEdit" /></div>
        <div class="field"><label for="fixed-cost">Custo fixo mensal</label><NumberInput id="fixed-cost" v-model="financials.monthlyFixedCost" :disabled="!canEdit" /></div>
        <div class="field"><label for="net-debt">Dívida líquida</label><NumberInput id="net-debt" v-model="financials.netDebt" :disabled="!canEdit" /></div>
        <div class="field"><label for="ebitda">EBITDA</label><NumberInput id="ebitda" v-model="financials.ebitda" :disabled="!canEdit" /></div>
        <div class="field"><label for="usd-debt">Dívida em US$</label><NumberInput id="usd-debt" v-model="financials.usdDebt" :disabled="!canEdit" /></div>
        <div class="field">
          <label for="reference-date">Data-base</label>
          <input id="reference-date" v-model="financials.referenceDate" class="input" type="date" :disabled="!canEdit" />
        </div>
        <div class="field">
          <label>Alavancagem (DL/EBITDA)</label>
          <input class="input" :value="financials.leverage === null ? '—' : `${formatNumber(financials.leverage)}x`" disabled />
        </div>
      </div>
      <footer v-if="canEdit" class="form-foot">
        <button class="btn btn-primary" type="submit" :disabled="financialsSubmit.submitting.value">Salvar dados financeiros</button>
      </footer>
    </form>
  </section>

  <BaseModal v-if="commodityModal" :title="commodityModal.editing ? 'Editar commodity' : 'Nova commodity'" @close="commodityModal = null">
    <form id="commodity-form" class="stack" @submit.prevent="saveCommodity">
      <p v-if="commoditySubmit.error.value" class="alert alert-error">{{ commoditySubmit.error.value }}</p>
      <div class="field">
        <label for="commodity">Commodity</label>
        <select id="commodity" v-model="commodityForm.commodity" class="input" :disabled="!!commodityModal.editing">
          <option v-for="c in availableCommodities" :key="c" :value="c">{{ commodityLabel[c] }}</option>
        </select>
      </div>
      <div class="form-grid">
        <div class="field">
          <label for="capacity">Capacidade</label>
          <NumberInput id="capacity" v-model="commodityForm.capacity" required />
        </div>
        <div class="field">
          <label for="unit">Unidade</label>
          <select id="unit" v-model="commodityForm.unit" class="input">
            <option v-for="u in MEASUREMENT_UNITS" :key="u" :value="u">{{ unitLabel[u] }}</option>
          </select>
        </div>
        <div class="field">
          <label for="price-ref">Referência de preço</label>
          <input id="price-ref" v-model="commodityForm.priceReference" class="input" placeholder="NY11 · ICE" maxlength="64" />
        </div>
        <div class="field">
          <label for="currency">Moeda</label>
          <input id="currency" v-model="commodityForm.currency" class="input" maxlength="3" required />
        </div>
      </div>
      <label class="row"><input v-model="commodityForm.sells" type="checkbox" /> Vende esta commodity</label>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="commodityModal = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="commodity-form" :disabled="commoditySubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>
</template>

<style scoped>
/* Barra de seções em largura total, como na política. */
.setup-tabs {
  width: 100%;
}

.setup-tabs .tab {
  flex: 1 0 auto;
  justify-content: center;
}

.dirty {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--warning);
}

.hint {
  max-width: 760px;
  margin: 4px 0 0;
}

.form-foot {
  display: flex;
  justify-content: flex-end;
  margin: 4px -22px -20px;
  padding: 14px 22px;
  border-top: 1px solid var(--border);
}

@media (max-width: 720px) {
  .form-foot {
    margin: 4px -16px -16px;
    padding: 12px 16px;
  }

  .form-foot .btn {
    flex: 1;
  }
}
</style>
