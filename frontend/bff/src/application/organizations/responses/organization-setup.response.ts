import type { Sector } from '../../../cross-cutting/enums/sector.js';
import type { CommodityResponse } from './commodity.response.js';

/** Setup da companhia (tela Setup): identificação, capacidade, orçamento, financeiro e commodities. */
export interface OrganizationSetupResponse {
  id: string;
  name: string;
  slug: string;
  profile: {
    corporateName: string;
    taxId: string | null;
    headquarters: string | null;
    group: string | null;
    sector: Sector;
    cropYearStartMonth: number;
    activeCrop: string | null;
  };
  industrial: {
    millingCapacity: number | null;
    mixMinPct: number | null;
    mixMaxPct: number | null;
    mixGuidancePct: number | null;
  };
  /** Orçamento e gatilhos em ¢/lb-equivalente. */
  budget: {
    cashCost: number | null;
    economicFloor: number | null;
    equivalentPrice: number | null;
    targetMarginPct: number | null;
  };
  financials: {
    cash: number | null;
    creditLines: number | null;
    monthlyFixedCost: number | null;
    netDebt: number | null;
    ebitda: number | null;
    usdDebt: number | null;
    referenceDate: string | null;
    /** Dívida líquida / EBITDA. */
    leverage: number | null;
  };
  commodities: CommodityResponse[];
}
