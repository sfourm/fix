export const COMMODITIES = ['RawSugar', 'WhiteSugar', 'HydratedEthanol', 'AnhydrousEthanol', 'Corn', 'Soybean'] as const;

export type Commodity = (typeof COMMODITIES)[number];
