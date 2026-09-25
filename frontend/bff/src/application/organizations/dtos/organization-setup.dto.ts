import type { Sector } from '../../../cross-cutting/enums/sector.js';
import type { CommodityDto } from './commodity.dto.js';

/** Setup completo da companhia como vem do core. */
export interface OrganizationSetupDto {
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
    leverage: number | null;
  };
  commodities: CommodityDto[];
}
