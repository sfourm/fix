<script setup lang="ts">
import { computed } from 'vue';
import OrderTable from '../../../components/order/OrderTable.vue';
import { usePolicyContext } from '../policy-context';

/** Boletas aprovadas desta política cujo confirmation ainda não foi conferido (middle office). */
const { policy, work, reloadWork } = usePolicyContext();
const overdue = computed(() => work.value?.awaitingConfirmation.filter((o) => o.confirmationOverdue).length ?? 0);
</script>

<template>
  <div v-if="!work" class="card card-body muted">Carregando confirmações…</div>
  <div v-else class="stack">
    <div class="toolbar">
      <p class="lead small">Confirmações que precisam chegar e bater com a boleta. Divergências e recusas exigem reconciliação do middle office.</p>
      <button class="btn" @click="reloadWork">Atualizar</button>
    </div>

    <section class="card">
      <header class="card-header">
        <h2>Divergentes e recusados</h2>
        <span class="badge" :class="work.confirmationProblems.length ? 'badge-danger' : ''">{{ work.confirmationProblems.length }}</span>
      </header>
      <OrderTable v-if="work.confirmationProblems.length" :orders="work.confirmationProblems" :policy-id="policy.id" show-mandate />
      <p v-else class="card-body muted" style="margin: 0">Nenhuma divergência em aberto.</p>
    </section>

    <section class="card">
      <header class="card-header">
        <h2>Aguardando confirmation</h2>
        <div class="row">
          <span v-if="overdue" class="badge badge-danger">{{ overdue }} atrasado(s)</span>
          <span class="badge" :class="work.awaitingConfirmation.length ? 'badge-warning' : ''">{{ work.awaitingConfirmation.length }}</span>
        </div>
      </header>
      <OrderTable v-if="work.awaitingConfirmation.length" :orders="work.awaitingConfirmation" :policy-id="policy.id" show-mandate />
      <p v-else class="card-body muted" style="margin: 0">Todas as confirmações foram conferidas.</p>
    </section>
  </div>
</template>

<style scoped>
.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}
</style>
