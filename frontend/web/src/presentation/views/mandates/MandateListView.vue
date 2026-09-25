<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { mandateStatusLabel } from '@/domain/labels';
import { MANDATE_STATUSES, type MandateStatus } from '@/domain/mandate';
import { Permission } from '@/domain/permissions';
import { useLoader } from '../../composables/useAsync';
import MandateTable from '../../components/mandate/MandateTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';

const api = useApi();
const organization = useOrganizationStore();

const page = ref(1);
const status = ref<MandateStatus | ''>('');
const { data, loading, error, load } = useLoader(() =>
  api.mandates.list({ page: page.value, pageSize: 20, status: status.value || undefined }),
);

function filter() {
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
      <select v-model="status" class="input" style="width: auto" aria-label="Filtrar por status" @change="filter">
        <option value="">Todos os status</option>
        <option v-for="s in MANDATE_STATUSES" :key="s" :value="s">{{ mandateStatusLabel[s] }}</option>
      </select>
      <RouterLink v-if="organization.can(Permission.CreateMandate)" class="btn btn-primary" to="/mandates/new">+ Novo mandato</RouterLink>
    </template>
  </PageHeader>

  <section class="card">
    <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhum mandato encontrado." @retry="load">
      <MandateTable :mandates="data?.items ?? []" show-policy />
      <PaginationBar v-if="data" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
    </StateBlock>
  </section>
</template>
