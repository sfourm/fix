import type { OptionKind } from '../../../cross-cutting/enums/option-kind.js';
import type { OrderType } from '../../../cross-cutting/enums/order-type.js';
import type { TradeDirection } from '../../../cross-cutting/enums/trade-direction.js';

export interface OrderTermsResponse {
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
  /** Venda de opção coberta. */
  coveredSale: boolean;
}
