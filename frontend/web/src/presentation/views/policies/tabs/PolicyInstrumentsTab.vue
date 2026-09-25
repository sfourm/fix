<script setup lang="ts">
import PolicyInstrumentsPanel from '../../../components/policy/PolicyInstrumentsPanel.vue';
import { usePolicyContext } from '../policy-context';

const { policy, editable, onUpdated } = usePolicyContext();
</script>

<template>
  <section class="card">
    <header class="card-header">
      <h2>Instrumentos permitidos</h2>
      <span class="muted small">permitido · com teto · vedado</span>
    </header>
    <PolicyInstrumentsPanel :policy="policy" :editable="editable" @updated="onUpdated" />
  </section>
</template>
