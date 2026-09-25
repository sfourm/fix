<script setup lang="ts">
import PolicyAxesPanel from '../../../components/policy/PolicyAxesPanel.vue';
import PolicyBandsPanel from '../../../components/policy/PolicyBandsPanel.vue';
import { usePolicyContext } from '../policy-context';

const { policy, editable, onUpdated } = usePolicyContext();
</script>

<template>
  <div class="stack">
    <section class="card">
      <header class="card-header">
        <h2>Eixos por fator de risco</h2>
        <span class="muted small">cada mandato é emitido sobre um eixo compatível com o seu tipo</span>
      </header>
      <PolicyAxesPanel :policy="policy" :editable="editable" @updated="onUpdated" />
    </section>
    <section class="card">
      <header class="card-header">
        <h2>Bandas de cobertura</h2>
        <span class="muted small">faixa mínima e máxima de hedge por safra</span>
      </header>
      <PolicyBandsPanel :policy="policy" :editable="editable" @updated="onUpdated" />
    </section>
  </div>
</template>
