import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { OptionKind } from '../../../cross-cutting/enums/option-kind.js';
import type { OrderType } from '../../../cross-cutting/enums/order-type.js';
import type { TradeDirection } from '../../../cross-cutting/enums/trade-direction.js';

export interface OrderTermsInput {
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
  /** Só sem mandato (com mandato vale a commodity dele). */
  commodity: Commodity | null;
  /** Venda de opção coberta; a descoberta é vedada (FORA). */
  coveredSale: boolean;
  /** Obrigatória quando há desvio: sem mandato, estouro de saldo, tela diferente, venda descoberta. */
  justification: string | null;
}
