<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useApi } from '@/application/api-provider';
import { useQueueStore } from '@/application/stores/queue.store';
import type { Order } from '@/domain/order';
import { useLoader } from '../../composables/useAsync';
import OrderTable from '../../components/order/OrderTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';

const api = useApi();
const queue = useQueueStore();

/** Boletas aprovadas cujo confirmation ainda não foi conferido: pendentes, divergentes e recusados. */
const { data, loading, error, load } = useLoader(async () => {
  const [pending, divergent, refused] = await Promise.all([
    api.orders.list({ approval: 'Approved', confirmation: 'Pending', pageSize: 100 }),
    api.orders.list({ confirmation: 'Divergent', pageSize: 100 }),
    api.orders.list({ confirmation: 'Refused', pageSize: 100 }),
  ]);
  return { pending: pending.items, problems: [...divergent.items, ...refused.items] };
});

const overdue = computed(() => data.value?.pending.filter((o: Order) => o.confirmationOverdue).length ?? 0);

onMounted(() => {
  load();
  queue.refresh();
});
</script>

<template>
  <PageHeader title="Boletas em aberto" subtitle="Confirmations que precisam chegar e bater com a boleta. Divergências e recusas exigem reconciliação do middle office.">
    <template #actions>
      <button class="btn" @click="load">Atualizar</button>
    </template>
  </PageHeader>

  <StateBlock :loading="loading && !data" :error="error" @retry="load">
    <div v-if="data" class="stack">
      <section class="card">
        <header class="card-header">
          <h2>Divergentes e recusados</h2>
          <span class="badge badge-danger">{{ data.problems.length }}</span>
        </header>
        <OrderTable v-if="data.problems.length" :orders="data.problems" show-mandate />
        <p v-else class="card-body muted" style="margin: 0">Nenhuma divergência em aberto.</p>
      </section>

      <section class="card">
        <header class="card-header">
          <h2>Aguardando confirmation</h2>
          <div class="row">
            <span v-if="overdue" class="badge badge-danger">{{ overdue }} atrasado(s)</span>
            <span class="badge badge-warning">{{ data.pending.length }}</span>
          </div>
        </header>
        <OrderTable v-if="data.pending.length" :orders="data.pending" show-mandate />
        <p v-else class="card-body muted" style="margin: 0">Todos os confirmations foram conferidos.</p>
      </section>
    </div>
  </StateBlock>
</template>
