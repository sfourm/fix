<script setup lang="ts">
import { watch } from 'vue';
import { directionLabel, optionKindLabel, orderTypeLabel } from '@/domain/labels';
import { OPTION_KINDS, TRADE_DIRECTIONS, type OrderTermsDraft, type OrderType } from '@/domain/order';
import NumberInput from '../NumberInput.vue';

/** Termos da boleta; os campos mudam com o instrumento (futuro/opção em lotes, NDF em nocional US$). */
const terms = defineModel<OrderTermsDraft>({ required: true });
const props = defineProps<{ types: OrderType[]; fieldError?: (name: string) => string | undefined }>();

watch(
  () => props.types,
  (types) => {
    if (!types.includes(terms.value.type) && types[0]) terms.value.type = types[0];
  },
  { immediate: true },
);

watch(
  () => terms.value.type,
  (type) => {
    if (type === 'Ndf') {
      terms.value.lots = null;
      terms.value.optionKind = null;
      terms.value.premium = null;
      terms.value.priceUnit ||= 'R$/US$';
    } else {
      terms.value.notionalUsd = null;
      if (type === 'Futures') {
        terms.value.optionKind = null;
        terms.value.premium = null;
        terms.value.coveredSale = false;
      } else {
        terms.value.optionKind ??= 'Put';
      }
    }
  },
);
</script>

<template>
  <div class="stack">
    <div class="form-grid wide">
      <div class="field">
        <label for="order-type">Instrumento</label>
        <select id="order-type" v-model="terms.type" class="input">
          <option v-for="t in types" :key="t" :value="t">{{ orderTypeLabel[t] }}</option>
        </select>
      </div>
      <div class="field">
        <label for="order-direction">Operação</label>
        <select id="order-direction" v-model="terms.direction" class="input">
          <option v-for="d in TRADE_DIRECTIONS" :key="d" :value="d">{{ directionLabel[d] }}</option>
        </select>
      </div>
      <div class="field">
        <label for="order-tenor">Vencimento / tela</label>
        <input id="order-tenor" v-model="terms.tenor" class="input" maxlength="16" placeholder="N27" required />
      </div>
      <div class="field">
        <label for="order-trade-date">Data da operação</label>
        <input id="order-trade-date" v-model="terms.tradeDate" class="input" type="date" required />
      </div>
    </div>

    <div class="form-grid wide">
      <div v-if="terms.type === 'Ndf'" class="field">
        <label for="order-notional">Nocional (US$)</label>
        <NumberInput id="order-notional" v-model="terms.notionalUsd" :min="0" required />
      </div>
      <div v-else class="field">
        <label for="order-lots">Lotes</label>
        <NumberInput id="order-lots" v-model="terms.lots" :min="0" required />
      </div>
      <div class="field">
        <label for="order-price">{{ terms.type === 'Option' ? 'Strike' : terms.type === 'Ndf' ? 'Taxa' : 'Preço' }}</label>
        <NumberInput id="order-price" v-model="terms.price" :min="0" required />
      </div>
      <div class="field">
        <label for="order-price-unit">Unidade</label>
        <input id="order-price-unit" v-model="terms.priceUnit" class="input" maxlength="16" placeholder="c/lb" />
      </div>
      <template v-if="terms.type === 'Option'">
        <div class="field">
          <label for="order-option-kind">Tipo da opção</label>
          <select id="order-option-kind" v-model="terms.optionKind" class="input">
            <option v-for="k in OPTION_KINDS" :key="k" :value="k">{{ optionKindLabel[k] }}</option>
          </select>
        </div>
        <div class="field">
          <label for="order-premium">Prêmio</label>
          <NumberInput id="order-premium" v-model="terms.premium" required />
        </div>
      </template>
    </div>

    <!-- Venda de opção: coberta (lastreada na produção/posição) é permitida até o teto; descoberta é vedada (FIX2 · I-08). -->
    <label v-if="terms.type === 'Option' && terms.direction === 'Sell'" class="row covered">
      <input v-model="terms.coveredSale" type="checkbox" />
      <span>
        Venda <strong>coberta</strong> (lastreada na produção ou numa posição)
        <span class="muted small">— sem cobertura, a venda de opção é vedada pela política e fica FORA.</span>
      </span>
    </label>

    <div class="field">
      <label for="order-notes">Observações</label>
      <textarea id="order-notes" v-model="terms.notes" class="input" maxlength="1000" />
    </div>
  </div>
</template>

<style scoped>
.covered {
  align-items: flex-start;
  gap: 10px;
  font-size: 0.92rem;
}
</style>
