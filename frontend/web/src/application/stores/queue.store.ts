import { defineStore } from 'pinia';
import { ref } from 'vue';
import { Permission } from '@/domain/permissions';
import { useApi } from '../api-provider';
import { useOrganizationStore } from './organization.store';

/** Contadores das filas de trabalho exibidos no menu (aprovações e confirmações em aberto). */
export const useQueueStore = defineStore('queue', () => {
  const pendingApprovals = ref(0);
  const openConfirmations = ref(0);

  async function refresh() {
    const api = useApi();
    const organization = useOrganizationStore();
    const count = (promise: Promise<{ totalCount: number }>) => promise.then((p) => p.totalCount).catch(() => 0);

    const [mandates, orders, pending, divergent, refused] = await Promise.all([
      organization.can(Permission.ViewMandate) ? count(api.mandates.list({ status: 'PendingApproval', pageSize: 1 })) : 0,
      organization.can(Permission.ViewOrder) ? count(api.orders.list({ approval: 'PendingApproval', pageSize: 1 })) : 0,
      organization.can(Permission.ViewOrder) ? count(api.orders.list({ approval: 'Approved', confirmation: 'Pending', pageSize: 1 })) : 0,
      organization.can(Permission.ViewOrder) ? count(api.orders.list({ confirmation: 'Divergent', pageSize: 1 })) : 0,
      organization.can(Permission.ViewOrder) ? count(api.orders.list({ confirmation: 'Refused', pageSize: 1 })) : 0,
    ]);

    pendingApprovals.value = mandates + orders;
    openConfirmations.value = pending + divergent + refused;
  }

  function reset() {
    pendingApprovals.value = 0;
    openConfirmations.value = 0;
  }

  return { pendingApprovals, openConfirmations, refresh, reset };
});
