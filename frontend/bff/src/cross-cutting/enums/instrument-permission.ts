export const INSTRUMENT_PERMISSIONS = ['Allowed', 'Capped', 'Forbidden'] as const;

export type InstrumentPermission = (typeof INSTRUMENT_PERMISSIONS)[number];
