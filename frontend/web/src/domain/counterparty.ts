export const COUNTERPARTY_TYPES = ['Trading', 'BankTrading', 'OtcCounterparty', 'Broker', 'Carrier', 'Producer'] as const;
export type CounterpartyType = (typeof COUNTERPARTY_TYPES)[number];

export interface Counterparty {
  id: string;
  /** CP-01 */
  code: string;
  name: string;
  type: CounterpartyType;
  document: string | null;
  address: string | null;
  country: string | null;
  isHomologated: boolean;
  notionalLimitUsd: number | null;
  mtmLimitUsd: number | null;
}

export type CounterpartyInput = Omit<Counterparty, 'id' | 'code' | 'isHomologated'>;
