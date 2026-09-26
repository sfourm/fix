<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import type { Mandate } from '@/domain/mandate';
import { Permission } from '@/domain/permissions';
import type { FilterCriterion } from '@/domain/search';
import { useLoader } from '../../../composables/useAsync';
import FilterBar from '../../../components/filters/FilterBar.vue';
import MandateTable from '../../../components/mandate/MandateTable.vue';
import PaginationBar from '../../../components/PaginationBar.vue';
import StateBlock from '../../../components/StateBlock.vue';
import { paths } from '../../../paths';
import { usePolicyContext } from '../policy-context';

/** Mandatos desta política, com a barra de filtros (filtros salvos inclusos) sempre presa à política. */
const { policy } = usePolicyContext();
const api = useApi();
const organization = useOrganizationStore();

const page = ref(1);
const search = ref<{ filterId: string | null; criteria: FilterCriterion[] }>({ filterId: null, criteria: [] });
const { data, loading, error, load } = useLoader(() =>
  api.search.run<Mandate>('mandates', {
    filterId: search.value.filterId,
    criteria: [{ field: 'policyId', operator: 'eq', value: policy.value.id }, ...search.value.criteria],
    page: page.value,
    pageSize: 20,
  }),
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
  <div class="stack">
    <div class="toolbar">
      <p class="lead small">Mandatos autorizam volume dentro de um eixo desta política. Dentro da política e com alçada de emissão entram ativos; fora dela exigem aprovação de exceção.</p>
      <div v-if="organization.can(Permission.CreateMandate) && policy.status === 'Active'" class="row">
        <RouterLink class="btn" :to="paths.uploadKind('Mandates', true)">Importar planilha</RouterLink>
        <RouterLink class="btn btn-primary" :to="paths.newMandate(policy.id)">+ Emitir mandato</RouterLink>
      </div>
    </div>
    <p v-if="policy.status !== 'Active'" class="alert alert-info">Mandatos só podem ser emitidos em política vigente.</p>

    <FilterBar source="mandates" @change="filter" />

    <section class="card">
      <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhum mandato nesta política." @retry="load">
        <MandateTable :mandates="data?.items ?? []" />
        <PaginationBar v-if="data" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
      </StateBlock>
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
