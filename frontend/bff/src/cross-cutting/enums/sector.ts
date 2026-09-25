export const SECTORS = ['SugarEnergy', 'Grains', 'Livestock'] as const;

export type Sector = (typeof SECTORS)[number];
