import { computed, inject, type ComputedRef, type InjectionKey, type Ref } from 'vue';
import type { Mandate } from '@/domain/mandate';
import type { Order } from '@/domain/order';
import type { Policy } from '@/domain/policy';

/** Trabalho em aberto da política: o que alimenta as abas de aprovações e confirmations e os contadores. */
export interface PolicyWork {
  mandates: Mandate[];
  /** Mandatos e boletas aguardando decisão. */
  pendingMandates: Mandate[];
  pendingOrders: Order[];
  /** Boletas aprovadas cujo confirmation não foi conferido: pendentes e divergentes/recusados. */
  awaitingConfirmation: Order[];
  confirmationProblems: Order[];
}

/** Contexto que a tela da política (PolicyShellView) entrega às abas. */
export interface PolicyContext {
  policy: Ref<Policy>;
  editable: ComputedRef<boolean>;
  work: Ref<PolicyWork | null>;
  /** Incrementa quando a política muda (abas com histórico recarregam). */
  refreshKey: Ref<number>;
  onUpdated: (policy: Policy) => void;
  reloadWork: () => Promise<void>;
}

export const policyContextKey: InjectionKey<PolicyContext> = Symbol('policy');

export function usePolicyContext(): PolicyContext {
  const ctx = inject(policyContextKey);
  if (!ctx) throw new Error('Aba de política usada fora de PolicyShellView.');
  return ctx;
}

/** Contadores das abas (aprovações e confirmations em aberto). */
export function workCounts(work: Ref<PolicyWork | null>) {
  return {
    approvals: computed(() => (work.value ? work.value.pendingMandates.length + work.value.pendingOrders.length : 0)),
    confirmations: computed(() => (work.value ? work.value.awaitingConfirmation.length + work.value.confirmationProblems.length : 0)),
  };
}
