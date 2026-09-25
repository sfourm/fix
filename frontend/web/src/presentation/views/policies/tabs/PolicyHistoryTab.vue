<script setup lang="ts">
import { ref } from 'vue';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import { formatDate } from '../../../composables/format';
import PolicyVersionSheet from '../../../components/policy/PolicyVersionSheet.vue';
import StatusBadge from '../../../components/StatusBadge.vue';
import { usePolicyContext } from '../policy-context';

const { policy } = usePolicyContext();

/** Versão aberta no modal (ciclo, alterações e PDF). */
const selected = ref<string | null>(null);
</script>

<template>
  <section class="card">
    <header class="card-header">
      <h2>Versões</h2>
      <span class="muted small">clique numa versão para ver o ciclo de aprovação, o que mudou e exportar em PDF</span>
    </header>
    <div class="table-wrap">
      <table v-columns="'policy-history'" class="table">
        <thead><tr><th>Versão</th><th>Status</th><th>Data</th><th>Nota</th></tr></thead>
        <tbody>
          <tr v-for="(v, i) in policy.versions" :key="`${v.version}-${i}`" class="clickable" @click="selected = v.version">
            <td class="num"><strong>{{ v.version }}</strong></td>
            <td><StatusBadge :label="policyStatusLabel[v.status]" :tone="policyStatusTone[v.status]" /></td>
            <td class="num">{{ formatDate(v.date) }}</td>
            <td class="muted">{{ v.note ?? '—' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <PolicyVersionSheet v-if="selected" :policy="policy" :version="selected" @select="(v) => (selected = v)" @close="selected = null" />
</template>
