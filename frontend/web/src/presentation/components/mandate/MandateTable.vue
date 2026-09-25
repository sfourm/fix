<script setup lang="ts">
import { useRouter } from 'vue-router';
import { complianceLabel, complianceTone, mandateStatusLabel, mandateStatusTone, mandateTypeLabel, unitLabel } from '@/domain/labels';
import type { Mandate } from '@/domain/mandate';
import { formatNumber } from '../../composables/format';
import { paths } from '../../paths';
import StatusBadge from '../StatusBadge.vue';

defineProps<{ mandates: Mandate[]; showPolicy?: boolean }>();
const router = useRouter();
</script>

<template>
  <div class="table-wrap">
    <table v-columns="'mandates'" class="table">
      <thead>
        <tr>
          <th>Mandato</th>
          <th v-if="showPolicy">Política · eixo</th>
          <th>Tipo</th>
          <th>Tela</th>
          <th>Autorizado</th>
          <th>Saldo</th>
          <th>Enquadramento</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="m in mandates" :key="m.id" class="clickable" @click="router.push(paths.mandate(m.policyId, m.id))">
          <td><strong>{{ m.terms.title }}</strong></td>
          <td v-if="showPolicy" class="muted small">{{ m.policyCode.toUpperCase() }} {{ m.policyVersion }} · {{ m.axisCode }}</td>
          <td>{{ mandateTypeLabel[m.type] }}</td>
          <td>{{ m.terms.tenor ?? '—' }}</td>
          <td class="num">{{ m.terms.quantity === null ? 'sem teto' : `${formatNumber(m.terms.quantity)} ${unitLabel[m.terms.quantityUnit]}` }}</td>
          <td class="num">{{ m.balance === null ? '—' : formatNumber(m.balance) }}</td>
          <td><StatusBadge :label="complianceLabel[m.compliance.status]" :tone="complianceTone[m.compliance.status]" /></td>
          <td><StatusBadge :label="mandateStatusLabel[m.status]" :tone="mandateStatusTone[m.status]" /></td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
