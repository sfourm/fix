export const RISK_FACTORS = ['Physical', 'Price', 'Currency', 'Freight'] as const;

export type RiskFactor = (typeof RISK_FACTORS)[number];
