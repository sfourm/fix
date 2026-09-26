import type { Commodity } from './common';
import type { MandateType } from './mandate';

export const ORDER_TYPES = ['Futures', 'Option', 'Ndf'] as const;
export type OrderType = (typeof ORDER_TYPES)[number];

export const TRADE_DIRECTIONS = ['Buy', 'Sell'] as const;
export type TradeDirection = (typeof TRADE_DIRECTIONS)[number];

export const OPTION_KINDS = ['Call', 'Put'] as const;
export type OptionKind = (typeof OPTION_KINDS)[number];

export const APPROVAL_STATUSES = ['PendingApproval', 'Approved', 'Rejected'] as const;
export type ApprovalStatus = (typeof APPROVAL_STATUSES)[number];

export const CONFIRMATION_STATUSES = ['Pending', 'Confirmed', 'Divergent', 'Refused'] as const;
export type ConfirmationStatus = (typeof CONFIRMATION_STATUSES)[number];

/** Boletas executam mandatos de precificação (futuros/opções) e de moeda (NDF). */
export const orderTypesFor = (type: MandateType): OrderType[] =>
  type === 'Pricing' ? ['Futures', 'Option'] : type === 'Currency' ? ['Ndf'] : [];

export interface OrderTerms {
  type: OrderType;
  direction: TradeDirection;
  /** Vencimento: N26, fev/27. */
  tenor: string;
  /** Futuros e opções. */
  lots: number | null;
  /** NDF. */
  notionalUsd: number | null;
  /** Preço (futuro), taxa (NDF) ou strike (opção). */
  price: number;
  priceUnit: string | null;
  optionKind: OptionKind | null;
  premium: number | null;
  tradeDate: string;
  notes: string | null;
  /** Venda de opção coberta; a descoberta é vedada (FORA). */
  coveredSale: boolean;
}

/** Termos enviados ao registrar/editar: commodity só vale sem mandato; justificativa é obrigatória em desvio. */
export interface OrderTermsInput extends OrderTerms {
  commodity: Commodity | null;
  justification: string | null;
}

/** Enquadramento calculado da boleta (FIX2: desvio não bloqueia — expõe). */
export interface OrderCompliance {
  status: 'Within' | 'Outside';
  reason: string;
}

export interface Order {
  id: string;
  /** HX-0001 */
  code: string;
  /** Nulo = boleta sem mandato (desvio sinalizado). */
  mandateId: string | null;
  /** MD-01 */
  mandateCode: string | null;
  mandateTitle: string;
  counterpartyId: string;
  counterpartyName: string;
  /** Ausente em NDF / em mandatos que não são de precificação. */
  commodity: Commodity | null;
  terms: OrderTerms;
  approval: ApprovalStatus;
  requestedBy: string;
  decidedBy: string | null;
  decidedAt: string | null;
  decisionNote: string | null;
  confirmation: ConfirmationStatus;
  confirmedOn: string | null;
  confirmationNote: string | null;
  /** Pendente há mais de 2 dias úteis. */
  confirmationOverdue: boolean;
  /** Middle office que registrou a confirmação (nunca quem executou). */
  confirmationBy: string | null;
  compliance: OrderCompliance;
  /** Mandato vinculado depois da execução: carimbo permanente. */
  linkedAfterExecution: boolean;
  exceedsMandate: boolean;
  deviationNote: string | null;
}

/** Carimbos de desvio visíveis na boleta (FIX2 · I-01). */
export function orderStamps(order: Order): { label: string; tone: 'danger' | 'warning' | 'info' }[] {
  const stamps: { label: string; tone: 'danger' | 'warning' | 'info' }[] = [];
  if (!order.mandateId) stamps.push({ label: 'sem mandato', tone: 'danger' });
  if (order.linkedAfterExecution) stamps.push({ label: 'a posteriori', tone: 'warning' });
  if (order.exceedsMandate) stamps.push({ label: 'estourou o mandato', tone: 'danger' });
  if (order.compliance.status === 'Outside' && stamps.length === 0) stamps.push({ label: 'FORA', tone: 'danger' });
  return stamps;
}

/** Confirmação em aberto: boleta aprovada sem confirmação conferida. */
export const hasOpenConfirmation = (order: Order) => order.approval === 'Approved' && order.confirmation !== 'Confirmed';

/** Termos em edição no formulário (preço ainda pode estar vazio). */
export type OrderTermsDraft = Omit<OrderTerms, 'price'> & { price: number | null };
