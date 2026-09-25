<script setup lang="ts">
import { useRouter } from 'vue-router';
import {
  approvalLabel,
  approvalTone,
  confirmationLabel,
  confirmationTone,
  directionLabel,
  optionKindLabel,
  orderTypeLabel,
} from '@/domain/labels';
import type { Order } from '@/domain/order';
import { formatDate, formatNumber, formatUsd } from '../../composables/format';
import StatusBadge from '../StatusBadge.vue';
import { paths } from '../../paths';

/** policyId: as boletas sempre são abertas dentro da política (cadeia 1:N). */
const props = defineProps<{ orders: Order[]; policyId: string; showMandate?: boolean }>();
const router = useRouter();

const volume = (o: Order) => (o.terms.type === 'Ndf' ? formatUsd(o.terms.notionalUsd) : `${formatNumber(o.terms.lots)} lotes`);
</script>

<template>
  <div class="table-wrap">
    <table v-columns="'orders'" class="table">
      <thead>
        <tr>
          <th>Trade</th>
          <th v-if="showMandate">Mandato</th>
          <th>Instrumento</th>
          <th>Tela</th>
          <th>Volume</th>
          <th>Preço</th>
          <th>Contraparte</th>
          <th>Aprovação</th>
          <th>Confirmation</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="o in orders" :key="o.id" class="clickable" @click="router.push(paths.order(props.policyId, o.mandateId, o.id))">
          <td class="num">{{ formatDate(o.terms.tradeDate) }}</td>
          <td v-if="showMandate" class="small">{{ o.mandateTitle }}</td>
          <td>
            <strong>{{ directionLabel[o.terms.direction] }} {{ orderTypeLabel[o.terms.type] }}</strong>
            <span v-if="o.terms.optionKind" class="muted"> {{ optionKindLabel[o.terms.optionKind] }}</span>
          </td>
          <td>{{ o.terms.tenor }}</td>
          <td class="num">{{ volume(o) }}</td>
          <td class="num">{{ formatNumber(o.terms.price, 4) }} <span class="muted small">{{ o.terms.priceUnit }}</span></td>
          <td>{{ o.counterpartyName }}</td>
          <td><StatusBadge :label="approvalLabel[o.approval]" :tone="approvalTone[o.approval]" /></td>
          <td>
            <template v-if="o.approval === 'Approved'">
              <StatusBadge :label="confirmationLabel[o.confirmation]" :tone="confirmationTone[o.confirmation]" />
              <span v-if="o.confirmationOverdue" class="badge badge-danger" style="margin-left: 4px">atrasado</span>
            </template>
            <span v-else class="muted">—</span>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
