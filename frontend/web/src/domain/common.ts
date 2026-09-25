export const COMMODITIES = ['RawSugar', 'WhiteSugar', 'HydratedEthanol', 'AnhydrousEthanol', 'Corn', 'Soybean'] as const;
export type Commodity = (typeof COMMODITIES)[number];

export const MEASUREMENT_UNITS = ['Lots', 'Tonnes', 'Bags', 'CubicMeters', 'Pounds', 'Usd'] as const;
export type MeasurementUnit = (typeof MEASUREMENT_UNITS)[number];
