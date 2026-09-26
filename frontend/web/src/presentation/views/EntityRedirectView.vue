<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { errorMessage } from '@/infrastructure/http/api-error';
import { paths } from '../paths';
import StateBlock from '../components/StateBlock.vue';

/**
 * Endereços antigos (/mandates/:id, /orders/:id): descobre a política (e o mandato) e troca pelo caminho da cadeia 1:N.
 * Mantém funcionando links salvos, notificações e dashboards antigos.
 */
const props = defineProps<{ kind: 'mandate' | 'order'; id: string }>();

const api = useApi();
const router = useRouter();
const error = ref<string | null>(null);

onMounted(async () => {
  try {
    if (props.kind === 'mandate') {
      const m = await api.mandates.get(props.id);
      await router.replace(paths.mandate(m.policyId, m.id));
    } else {
      const o = await api.orders.get(props.id);
      if (!o.mandateId) {
        await router.replace(paths.orderWithoutMandate(o.id));
        return;
      }
      const m = await api.mandates.get(o.mandateId);
      await router.replace(paths.order(m.policyId, m.id, o.id));
    }
  } catch (e) {
    error.value = errorMessage(e);
  }
});
</script>

<template>
  <StateBlock :loading="!error" :error="error" />
</template>
