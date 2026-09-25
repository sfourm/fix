<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { approvalLabel, confirmationLabel } from '@/domain/labels';
import { APPROVAL_STATUSES, CONFIRMATION_STATUSES, type ApprovalStatus, type ConfirmationStatus } from '@/domain/order';
import { Permission } from '@/domain/permissions';
import { useLoader } from '../../composables/useAsync';
import OrderTable from '../../components/order/OrderTable.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';

const api = useApi();
const organization = useOrganizationStore();

const page = ref(1);
const approval = ref<ApprovalStatus | ''>('');
const confirmation = ref<ConfirmationStatus | ''>('');
const { data, loading, error, load } = useLoader(() =>
  api.orders.list({
    page: page.value,
    pageSize: 20,
    approval: approval.value || undefined,
    confirmation: confirmation.value || undefined,
  }),
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
  <PageHeader title="Boletas de hedge" subtitle="Fixações, opções e NDFs que executam os mandatos. Sem alçada, a boleta aguarda aprovação e não consome saldo.">
    <template #actions>
      <select v-model="approval" class="input" style="width: auto" aria-label="Filtrar por aprovação" @change="filter">
        <option value="">Toda aprovação</option>
        <option v-for="s in APPROVAL_STATUSES" :key="s" :value="s">{{ approvalLabel[s] }}</option>
      </select>
      <select v-model="confirmation" class="input" style="width: auto" aria-label="Filtrar por confirmation" @change="filter">
        <option value="">Todo confirmation</option>
        <option v-for="s in CONFIRMATION_STATUSES" :key="s" :value="s">{{ confirmationLabel[s] }}</option>
      </select>
      <RouterLink v-if="organization.can(Permission.CreateOrder)" class="btn btn-primary" to="/orders/new">+ Nova boleta</RouterLink>
    </template>
  </PageHeader>

  <section class="card">
    <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhuma boleta encontrada." @retry="load">
      <OrderTable :orders="data?.items ?? []" show-mandate />
      <PaginationBar v-if="data" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
    </StateBlock>
  </section>
</template>
