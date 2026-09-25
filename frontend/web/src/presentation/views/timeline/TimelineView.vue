<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { entityTypeLabel } from '@/domain/labels';
import type { Member } from '@/domain/organization';
import { Permission } from '@/domain/permissions';
import PageHeader from '../../components/PageHeader.vue';
import TimelineList from '../../components/TimelineList.vue';

const api = useApi();
const organization = useOrganizationStore();

const entityType = ref('');
const members = ref<Member[] | null>(null);
const refreshKey = ref(0);

const entityTypes = ['Organization', 'OrganizationGroup', 'Counterparty', 'Policy', 'Mandate', 'Order'];

onMounted(async () => {
  // Nomes dos autores só aparecem para quem pode ver os membros.
  if (organization.can(Permission.ViewUsers)) {
    members.value = await api.organizations.members().catch(() => null);
  }
});
</script>

<template>
  <PageHeader title="Timeline" subtitle="Trilha de auditoria de todas as alterações na organização. Alterações nas partes de um agregado (eixos, bandas, membros...) aparecem no histórico do agregado dono.">
    <template #actions>
      <select v-model="entityType" class="input" style="width: auto" aria-label="Filtrar por tipo">
        <option value="">Todas as entidades</option>
        <option v-for="type in entityTypes" :key="type" :value="type">{{ entityTypeLabel[type] ?? type }}</option>
      </select>
      <button class="btn" @click="refreshKey++">Atualizar</button>
    </template>
  </PageHeader>

  <section class="card">
    <TimelineList :entity-type="entityType || undefined" :limit="200" :members="members" :refresh-key="refreshKey" />
  </section>
</template>
