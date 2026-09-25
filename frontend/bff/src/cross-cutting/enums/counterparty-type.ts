export const COUNTERPARTY_TYPES = ['Trading', 'BankTrading', 'OtcCounterparty', 'Broker', 'Carrier', 'Producer'] as const;

export type CounterpartyType = (typeof COUNTERPARTY_TYPES)[number];
