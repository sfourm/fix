import type { Commodity, MeasurementUnit } from './common';

export const SECTORS = ['SugarEnergy', 'Grains', 'Livestock'] as const;
export type Sector = (typeof SECTORS)[number];

/** Instância (mesa) do membro na companhia. */
export const DESKS = ['ExecutionDesk', 'Commercial', 'Logistics', 'Board', 'RiskControl'] as const;
export type Desk = (typeof DESKS)[number];

export interface Organization {
  id: string;
  name: string;
  slug: string;
}

export interface CompanyProfile {
  corporateName: string;
  taxId: string | null;
  headquarters: string | null;
  group: string | null;
  sector: Sector;
  /** Mês de início do ano-safra (1–12). */
  cropYearStartMonth: number;
  /** AA/AA */
  activeCrop: string | null;
}

export interface IndustrialProfile {
  millingCapacity: number | null;
  mixMinPct: number | null;
  mixMaxPct: number | null;
  mixGuidancePct: number | null;
}

/** Orçamento e gatilhos em ¢/lb-equivalente. */
export interface Budget {
  cashCost: number | null;
  economicFloor: number | null;
  equivalentPrice: number | null;
  targetMarginPct: number | null;
}

export interface Financials {
  cash: number | null;
  creditLines: number | null;
  monthlyFixedCost: number | null;
  netDebt: number | null;
  ebitda: number | null;
  usdDebt: number | null;
  referenceDate: string | null;
  /** Dívida líquida / EBITDA (calculado pelo core). */
  leverage: number | null;
}

export interface OrganizationCommodity {
  id: string;
  commodity: Commodity;
  capacity: number;
  unit: MeasurementUnit;
  priceReference: string | null;
  currency: string;
  sells: boolean;
}

export interface OrganizationSetup extends Organization {
  profile: CompanyProfile;
  industrial: IndustrialProfile;
  budget: Budget;
  financials: Financials;
  commodities: OrganizationCommodity[];
}

export interface Member {
  id: string;
  userId: string;
  email: string;
  fullName: string;
  desk: Desk | null;
  rules: string[];
  groups: string[];
}

export interface Group {
  id: string;
  name: string;
  isDefault: boolean;
  rules: string[];
  memberIds: string[];
}
