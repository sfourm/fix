export const MANDATE_TYPES = ['Pricing', 'Currency', 'Commercial', 'Logistics'] as const;

export type MandateType = (typeof MANDATE_TYPES)[number];
