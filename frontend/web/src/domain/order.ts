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
}

export interface Order {
  id: string;
  mandateId: string;
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
}

/** Confirmation em aberto: boleta aprovada sem confirmation conferido. */
export const hasOpenConfirmation = (order: Order) => order.approval === 'Approved' && order.confirmation !== 'Confirmed';

/** Termos em edição no formulário (preço ainda pode estar vazio). */
export type OrderTermsDraft = Omit<OrderTerms, 'price'> & { price: number | null };
