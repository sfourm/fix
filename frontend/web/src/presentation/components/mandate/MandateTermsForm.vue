<script setup lang="ts">
import { COMMODITIES, MEASUREMENT_UNITS } from '@/domain/common';
import { commodityLabel, unitLabel } from '@/domain/labels';
import type { MandateTerms, MandateType } from '@/domain/mandate';
import NumberInput from '../NumberInput.vue';

/** Termos do mandato (o quê, quanto, a que preço e quando). Edita o objeto recebido em v-model. */
const terms = defineModel<MandateTerms>({ required: true });
defineProps<{ type: MandateType; fieldError?: (name: string) => string | undefined }>();
</script>

<template>
  <div class="stack">
    <div class="field">
      <label for="terms-title">Título do mandato</label>
      <input id="terms-title" v-model="terms.title" class="input" maxlength="200" placeholder="Fixar 30% da tela N27" required />
    </div>
    <div class="field">
      <label for="terms-criteria">Critério / racional</label>
      <textarea id="terms-criteria" v-model="terms.criteria" class="input" maxlength="1000" />
    </div>

    <h3 class="section-title">Volume</h3>
    <div class="form-grid wide">
      <div class="field">
        <label for="terms-commodity">Commodity</label>
        <select id="terms-commodity" v-model="terms.commodity" class="input">
          <option v-for="c in COMMODITIES" :key="c" :value="c">{{ commodityLabel[c] }}</option>
        </select>
      </div>
      <div class="field">
        <label for="terms-tenor">Tela / vencimento</label>
        <input id="terms-tenor" v-model="terms.tenor" class="input" maxlength="16" placeholder="N27" />
        <span v-if="fieldError?.('tenor')" class="field-error">{{ fieldError('tenor') }}</span>
      </div>
      <div class="field">
        <label for="terms-quantity">Quantidade autorizada</label>
        <NumberInput id="terms-quantity" v-model="terms.quantity" :min="0" />
      </div>
      <div class="field">
        <label for="terms-unit">Unidade</label>
        <select id="terms-unit" v-model="terms.quantityUnit" class="input">
          <option v-for="u in MEASUREMENT_UNITS" :key="u" :value="u">{{ unitLabel[u] }}</option>
        </select>
      </div>
    </div>
    <p v-if="type === 'Currency'" class="muted small" style="margin: -8px 0 0">Mandatos de moeda são expressos em nocional US$.</p>

    <h3 class="section-title">Preço</h3>
    <label class="row"><input v-model="terms.price.atMarket" type="checkbox" /> A mercado (sem preço-alvo)</label>
    <div v-if="!terms.price.atMarket" class="form-grid wide">
      <div class="field">
        <label for="price-min">Mínimo</label>
        <NumberInput id="price-min" v-model="terms.price.min" />
      </div>
      <div class="field">
        <label for="price-target">Target</label>
        <NumberInput id="price-target" v-model="terms.price.target" />
      </div>
      <div class="field">
        <label for="price-max">Máximo</label>
        <NumberInput id="price-max" v-model="terms.price.max" />
      </div>
      <div class="field">
        <label for="price-unit">Unidade de preço</label>
        <input id="price-unit" v-model="terms.price.unit" class="input" maxlength="16" placeholder="c/lb" list="price-units" />
        <datalist id="price-units">
          <option value="c/lb" />
          <option value="R$/t" />
          <option value="R$/US$" />
          <option value="US$/t" />
        </datalist>
      </div>
    </div>

    <h3 class="section-title">Janela de execução</h3>
    <div class="form-grid">
      <div class="field">
        <label for="window-start">Início</label>
        <input id="window-start" v-model="terms.windowStart" class="input" type="date" />
      </div>
      <div class="field">
        <label for="window-end">Fim</label>
        <input id="window-end" v-model="terms.windowEnd" class="input" type="date" />
        <span v-if="fieldError?.('terms.windowEnd')" class="field-error">{{ fieldError('terms.windowEnd') }}</span>
      </div>
    </div>
  </div>
</template>
