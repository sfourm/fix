<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useApi } from '@/application/api-provider';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import { useLoader } from '../../../composables/useAsync';
import { formatDate, formatDateTime } from '../../../composables/format';
import PolicyVersionSheet from '../../../components/policy/PolicyVersionSheet.vue';
import { timedVersions } from '../../../components/policy/policy-changes';
import StatusBadge from '../../../components/StatusBadge.vue';
import { usePolicyContext } from '../policy-context';

const { policy, refreshKey } = usePolicyContext();
const api = useApi();

/** Auditoria da política: dá o horário de cada etapa (a versão só guarda a data). */
const audit = useLoader(() => api.timeline.list({ entityType: 'Policy', entityId: policy.value.id, limit: 500 }));
const rows = computed(() => timedVersions(policy.value.versions, audit.data.value));

/** Versão aberta no modal (ciclo, alterações e PDF). */
const selected = ref<string | null>(null);

onMounted(audit.load);
watch(refreshKey, audit.load);
</script>

<template>
  <section class="card">
    <header class="card-header">
      <h2>Versões</h2>
      <span class="muted small">clique numa versão para ver o ciclo de aprovação, o que mudou e exportar em PDF</span>
    </header>
    <div class="table-wrap">
      <table v-columns="'policy-history'" class="table">
        <thead><tr><th>Versão</th><th>Status</th><th>Data e hora</th><th>Nota</th></tr></thead>
        <tbody>
          <tr v-for="(v, i) in rows" :key="`${v.version}-${i}`" class="clickable" @click="selected = v.version">
            <td class="num"><strong>{{ v.version }}</strong></td>
            <td><StatusBadge :label="policyStatusLabel[v.status]" :tone="policyStatusTone[v.status]" /></td>
            <td class="num">{{ v.at ? formatDateTime(v.at) : formatDate(v.date) }}</td>
            <td class="muted">{{ v.note ?? '—' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <PolicyVersionSheet v-if="selected" :policy="policy" :version="selected" @select="(v) => (selected = v)" @close="selected = null" />
</template>
