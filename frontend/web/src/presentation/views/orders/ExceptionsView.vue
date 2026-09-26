<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { Permission } from '@/domain/permissions';
import { useLoader } from '../../composables/useAsync';
import OrderTable from '../../components/order/OrderTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';
import { paths } from '../../paths';

/**
 * Exceções (FIX2 · I-01 "desvio não bloqueia — expõe"): boletas sem mandato e boletas FORA do enquadramento
 * (estouro de saldo, tela diferente, venda descoberta, vínculo a posteriori). Nada disso some: fica aqui até ser tratado.
 */
const api = useApi();
const organization = useOrganizationStore();
const route = useRoute();
const router = useRouter();

type Tab = 'without' | 'outside';
const tab = computed<Tab>(() => (route.query.tab === 'outside' ? 'outside' : 'without'));
const page = ref(1);
const PAGE_SIZE = 25;

const orders = useLoader(() =>
  api.orders.list({
    page: page.value,
    pageSize: PAGE_SIZE,
    ...(tab.value === 'without' ? { withoutMandate: true } : { onlyOutside: true }),
  }),
);
const counts = useLoader(async () => {
  const [without, outside] = await Promise.all([
    api.orders.list({ withoutMandate: true, pageSize: 1 }),
    api.orders.list({ onlyOutside: true, pageSize: 1 }),
  ]);
  return { without: without.totalCount, outside: outside.totalCount };
});

const tabs = computed(() => [
  { key: 'without' as const, label: 'Sem mandato', count: counts.data.value?.without },
  { key: 'outside' as const, label: 'FORA do enquadramento', count: counts.data.value?.outside },
]);
const select = (key: Tab) => router.replace({ query: key === 'without' ? {} : { tab: key } });

watch(tab, () => {
  page.value = 1;
  orders.load();
});
onMounted(() => {
  orders.load();
  counts.load();
});
</script>

<template>
  <PageHeader
    kicker="Políticas · desvios expostos"
    title="Exceções"
    subtitle="Boletas sem mandato ou FORA do enquadramento. O desvio é permitido com justificativa, mas nunca é silencioso: fica aqui, na fila de aprovação e com o carimbo na boleta."
  >
    <template #actions>
      <RouterLink v-if="organization.can(Permission.CreateOrder)" class="btn btn-primary" :to="paths.newOrderWithoutMandate()">+ Boleta sem mandato</RouterLink>
    </template>
  </PageHeader>

  <nav class="tabs exceptions-tabs" role="tablist" aria-label="Tipo de exceção">
    <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="tab" :class="{ active: tab === t.key }" :aria-selected="tab === t.key" @click="select(t.key)">
      {{ t.label }}
      <span v-if="t.count !== undefined" class="count">{{ t.count }}</span>
    </button>
  </nav>

  <section class="card">
    <StateBlock
      :loading="orders.loading.value && !orders.data.value"
      :error="orders.error.value"
      :empty="orders.data.value?.items.length === 0"
      :empty-text="tab === 'without' ? 'Nenhuma boleta sem mandato.' : 'Nenhuma boleta FORA do enquadramento.'"
      @retry="orders.load"
    >
      <OrderTable :orders="orders.data.value?.items ?? []" show-mandate />
      <PaginationBar
        v-if="orders.data.value"
        :page="orders.data.value.page"
        :page-size="orders.data.value.pageSize"
        :total-count="orders.data.value.totalCount"
        @change="(p) => { page = p; orders.load(); }"
      />
    </StateBlock>
  </section>
</template>

<style scoped>
.exceptions-tabs {
  width: 100%;
}

.exceptions-tabs .tab {
  flex: 1 0 auto;
  justify-content: center;
}
</style>
