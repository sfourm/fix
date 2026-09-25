export const ORDER_TYPES = ['Futures', 'Option', 'Ndf'] as const;

export type OrderType = (typeof ORDER_TYPES)[number];
