<script setup lang="ts">
import { computed } from 'vue';
import { mandateStatusLabel, mandateStatusTone, riskFactorLabel } from '@/domain/labels';
import { formatNumber } from '../../../composables/format';
import PolicyLimitsPanel from '../../../components/policy/PolicyLimitsPanel.vue';
import StatusBadge from '../../../components/StatusBadge.vue';
import { paths } from '../../../paths';
import { usePolicyContext } from '../policy-context';

/** Visão geral: eixos da política como cartões (com os mandatos de cada um) e os parâmetros gerais. */
const { policy, editable, work, onUpdated } = usePolicyContext();

const axes = computed(() =>
  policy.value.axes.map((axis) => {
    const mandates = work.value?.mandates.filter((m) => m.axisId === axis.id) ?? [];
    return { axis, mandates, active: mandates.filter((m) => m.status === 'Active').length };
  }),
);
</script>

<template>
  <div class="stack">
    <section v-if="axes.length" class="axes">
      <article v-for="{ axis, mandates, active } in axes" :key="axis.id" class="axis card">
        <header>
          <span class="factor" :data-factor="axis.factor">{{ riskFactorLabel[axis.factor] }}</span>
          <code>{{ axis.code }}</code>
        </header>
        <strong>{{ axis.title }}</strong>
        <small>{{ mandates.length }} mandato(s) · {{ active }} ativo(s)</small>
        <div v-if="mandates.length" class="links">
          <RouterLink v-for="m in mandates.slice(0, 3)" :key="m.id" :to="paths.mandate(policy.id, m.id)" class="link">
            <span>{{ m.terms.title }}</span>
            <StatusBadge :label="mandateStatusLabel[m.status]" :tone="mandateStatusTone[m.status]" />
          </RouterLink>
          <RouterLink v-if="mandates.length > 3" :to="paths.policy(policy.id, 'mandates')" class="more">+{{ mandates.length - 3 }} mandato(s)</RouterLink>
        </div>
      </article>
    </section>

    <section class="card">
      <header class="card-header">
        <h2>Parâmetros da política</h2>
        <span class="muted small">limites gerais · §6–§8{{ editable ? '' : ' · somente leitura' }}</span>
      </header>
      <PolicyLimitsPanel :policy="policy" :editable="editable" @updated="onUpdated" />
    </section>

    <p v-if="!policy.axes.length" class="alert alert-info">
      A política ainda não tem eixos. Cadastre os eixos por fator de risco em <RouterLink :to="paths.policy(policy.id, 'axes')">Eixos &amp; bandas</RouterLink>:
      os mandatos são emitidos sobre eles. Total de bandas: {{ formatNumber(policy.bands.length, 0) }}.
    </p>
  </div>
</template>

<style scoped>
.axes {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 12px;
}

.axis {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 14px 16px;
  transition:
    border-color 0.15s,
    transform 0.15s var(--ease);
}

.axis:hover {
  border-color: var(--primary);
  transform: translateY(-1px);
}

.axis header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.factor {
  padding: 1px 8px;
  border-radius: 6px;
  font-size: 0.68rem;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  background: var(--primary-soft);
  color: var(--primary);
}

.factor[data-factor='Physical'] {
  background: color-mix(in srgb, var(--steel) 14%, transparent);
  color: var(--steel);
}

.factor[data-factor='Currency'] {
  background: var(--success-soft);
  color: var(--success);
}

.factor[data-factor='Freight'] {
  background: color-mix(in srgb, var(--bronze) 14%, transparent);
  color: var(--bronze);
}

code {
  font-family: var(--font-mono);
  font-size: 0.75rem;
  color: var(--text-muted);
}

.axis strong {
  font-size: 0.98rem;
}

.axis small {
  color: var(--text-muted);
  font-size: 0.78rem;
}

.links {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 4px;
  padding-top: 8px;
  border-top: 1px solid var(--border);
}

.link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  color: var(--text-dim);
  font-size: 0.84rem;
}

.link span:first-child {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.link:hover {
  color: var(--primary);
}

.more {
  font-size: 0.78rem;
}
</style>
