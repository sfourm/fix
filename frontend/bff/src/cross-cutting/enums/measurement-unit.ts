export const MEASUREMENT_UNITS = ['Lots', 'Tonnes', 'Bags', 'CubicMeters', 'Pounds', 'Usd'] as const;

export type MeasurementUnit = (typeof MEASUREMENT_UNITS)[number];
