export const TRADE_DIRECTIONS = ['Buy', 'Sell'] as const;

export type TradeDirection = (typeof TRADE_DIRECTIONS)[number];
