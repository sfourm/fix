<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import type { Mandate } from '@/domain/mandate';
import { Permission } from '@/domain/permissions';
import type { FilterCriterion } from '@/domain/search';
import { useLoader } from '../../composables/useAsync';
import FilterBar from '../../components/filters/FilterBar.vue';
import MandateTable from '../../components/mandate/MandateTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';

const api = useApi();
const organization = useOrganizationStore();

const page = ref(1);
const search = ref<{ filterId: string | null; criteria: FilterCriterion[] }>({ filterId: null, criteria: [] });
const { data, loading, error, load } = useLoader(() => api.search.run<Mandate>('mandates', { ...search.value, page: page.value, pageSize: 20 }));

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
  <PageHeader
    title="Mandatos"
    subtitle="Autorizam volume dentro de um eixo da política. Dentro da política e com alçada de emissão entram ativos; fora da política exigem aprovação de exceção."
  >
    <template #actions>
      <RouterLink v-if="organization.can(Permission.CreateMandate)" class="btn btn-primary" to="/mandates/new">+ Novo mandato</RouterLink>
    </template>
  </PageHeader>

  <FilterBar source="mandates" @change="filter" />

  <section class="card">
    <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhum mandato encontrado." @retry="load">
      <MandateTable :mandates="data?.items ?? []" show-policy />
      <PaginationBar v-if="data" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
    </StateBlock>
  </section>
</template>
