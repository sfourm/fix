<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import type { Order } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import type { FilterCriterion } from '@/domain/search';
import { useLoader } from '../../composables/useAsync';
import FilterBar from '../../components/filters/FilterBar.vue';
import OrderTable from '../../components/order/OrderTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';

const api = useApi();
const organization = useOrganizationStore();

const page = ref(1);
const search = ref<{ filterId: string | null; criteria: FilterCriterion[] }>({ filterId: null, criteria: [] });
const { data, loading, error, load } = useLoader(() =>
  api.search.run<Order>('orders', { ...search.value, sort: { field: 'tradeDate', direction: 'desc' }, page: page.value, pageSize: 20 }),
);

function filter(value: { filterId: string | null; criteria: FilterCriterion[] }) {
  search.value = value;
  page.value = 1;
  load();
}

function goTo(p: number) {
  page.value = p;
  load();
}

onMounted(load);
</script>

<template>
  <PageHeader title="Boletas de hedge" subtitle="Fixações, opções e NDFs que executam os mandatos. Sem alçada, a boleta aguarda aprovação e não consome saldo.">
    <template #actions>
      <RouterLink v-if="organization.can(Permission.CreateOrder)" class="btn btn-primary" to="/orders/new">+ Nova boleta</RouterLink>
    </template>
  </PageHeader>

  <FilterBar source="orders" @change="filter" />

  <section class="card">
    <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhuma boleta encontrada." @retry="load">
      <OrderTable :orders="data?.items ?? []" show-mandate />
      <PaginationBar v-if="data" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
    </StateBlock>
  </section>
</template>
