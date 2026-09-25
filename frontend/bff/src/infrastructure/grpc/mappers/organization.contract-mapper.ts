import type { AddCommodityCommand } from '../../../application/organizations/commands/add-commodity.command.js';
import type { UpdateBudgetCommand } from '../../../application/organizations/commands/update-budget.command.js';
import type { UpdateCompanyProfileCommand } from '../../../application/organizations/commands/update-company-profile.command.js';
import type { UpdateFinancialsCommand } from '../../../application/organizations/commands/update-financials.command.js';
import type { UpdateIndustrialProfileCommand } from '../../../application/organizations/commands/update-industrial-profile.command.js';
import type { CommodityDto } from '../../../application/organizations/dtos/commodity.dto.js';
import type { GroupDto } from '../../../application/organizations/dtos/group.dto.js';
import type { MemberDto } from '../../../application/organizations/dtos/member.dto.js';
import type { OrganizationSetupDto } from '../../../application/organizations/dtos/organization-setup.dto.js';
import type { OrganizationDto } from '../../../application/organizations/dtos/organization.dto.js';
import { nullable, nullableNumber } from './common.contract-mapper.js';
import { commodityEnum, deskEnum, measurementUnitEnum, sectorEnum } from './enum.contract-mapper.js';

export interface ContractOrganization {
  id: string;
  name: string;
  slug: string;
}

export interface ContractCommodity {
  id: string;
  commodity: string;
  capacity: number;
  unit: string;
  priceReference?: string;
  currency: string;
  sells: boolean;
}

export interface ContractOrganizationSetup extends ContractOrganization {
  profile: {
    corporateName: string;
    taxId?: string;
    headquarters?: string;
    group?: string;
    sector: string;
    cropYearStartMonth: number;
    activeCrop?: string;
  } | null;
  industrial: { millingCapacity?: number; mixMinPct?: number; mixMaxPct?: number; mixGuidancePct?: number } | null;
  budget: { cashCost?: number; economicFloor?: number; equivalentPrice?: number; targetMarginPct?: number } | null;
  financials: {
    cash?: number;
    creditLines?: number;
    monthlyFixedCost?: number;
    netDebt?: number;
    ebitda?: number;
    usdDebt?: number;
    referenceDate?: string;
    leverage?: number;
  } | null;
  commodities: ContractCommodity[];
}

export interface ContractMember {
  id: string;
  userId: string;
  email: string;
  fullName: string;
  desk: string;
  rules: string[];
  groups: string[];
}

export interface ContractGroup {
  id: string;
  name: string;
  isDefault: boolean;
  rules: string[];
  memberIds: string[];
}

// ---------- contrato → dto ----------

export const toOrganizationDto = (o: ContractOrganization): OrganizationDto => ({ id: o.id, name: o.name, slug: o.slug });

export const toCommodityDto = (c: ContractCommodity): CommodityDto => ({
  id: c.id,
  commodity: commodityEnum.fromContractRequired(c.commodity),
  capacity: c.capacity,
  unit: measurementUnitEnum.fromContractRequired(c.unit),
  priceReference: nullable(c.priceReference),
  currency: c.currency,
  sells: c.sells,
});

export const toOrganizationSetupDto = (o: ContractOrganizationSetup): OrganizationSetupDto => {
  const profile = o.profile;
  const industrial = o.industrial ?? {};
  const budget = o.budget ?? {};
  const financials = o.financials ?? {};

  return {
    ...toOrganizationDto(o),
    profile: {
      corporateName: profile?.corporateName ?? o.name,
      taxId: nullable(profile?.taxId),
      headquarters: nullable(profile?.headquarters),
      group: nullable(profile?.group),
      sector: sectorEnum.fromContract(profile?.sector) ?? 'SugarEnergy',
      cropYearStartMonth: profile?.cropYearStartMonth || 4,
      activeCrop: nullable(profile?.activeCrop),
    },
    industrial: {
      millingCapacity: nullableNumber(industrial.millingCapacity),
      mixMinPct: nullableNumber(industrial.mixMinPct),
      mixMaxPct: nullableNumber(industrial.mixMaxPct),
      mixGuidancePct: nullableNumber(industrial.mixGuidancePct),
    },
    budget: {
      cashCost: nullableNumber(budget.cashCost),
      economicFloor: nullableNumber(budget.economicFloor),
      equivalentPrice: nullableNumber(budget.equivalentPrice),
      targetMarginPct: nullableNumber(budget.targetMarginPct),
    },
    financials: {
      cash: nullableNumber(financials.cash),
      creditLines: nullableNumber(financials.creditLines),
      monthlyFixedCost: nullableNumber(financials.monthlyFixedCost),
      netDebt: nullableNumber(financials.netDebt),
      ebitda: nullableNumber(financials.ebitda),
      usdDebt: nullableNumber(financials.usdDebt),
      referenceDate: nullable(financials.referenceDate),
      leverage: nullableNumber(financials.leverage),
    },
    commodities: o.commodities.map(toCommodityDto),
  };
};

export const toMemberDto = (m: ContractMember): MemberDto => ({
  id: m.id,
  userId: m.userId,
  email: m.email,
  fullName: m.fullName,
  desk: deskEnum.fromContract(m.desk),
  rules: m.rules,
  groups: m.groups,
});

export const toGroupDto = (g: ContractGroup): GroupDto => ({
  id: g.id,
  name: g.name,
  isDefault: g.isDefault,
  rules: g.rules,
  memberIds: g.memberIds,
});

// ---------- command → contrato (campos optional nulos são omitidos pelo proto-loader) ----------

export const toContractProfile = (c: UpdateCompanyProfileCommand) => ({
  corporateName: c.corporateName,
  taxId: c.taxId,
  headquarters: c.headquarters,
  group: c.group,
  sector: sectorEnum.toContract(c.sector),
  cropYearStartMonth: c.cropYearStartMonth,
  activeCrop: c.activeCrop,
});

export const toContractIndustrial = (c: UpdateIndustrialProfileCommand) => ({
  millingCapacity: c.millingCapacity,
  mixMinPct: c.mixMinPct,
  mixMaxPct: c.mixMaxPct,
  mixGuidancePct: c.mixGuidancePct,
});

export const toContractBudget = (c: UpdateBudgetCommand) => ({
  cashCost: c.cashCost,
  economicFloor: c.economicFloor,
  equivalentPrice: c.equivalentPrice,
  targetMarginPct: c.targetMarginPct,
});

export const toContractFinancials = (c: UpdateFinancialsCommand) => ({
  cash: c.cash,
  creditLines: c.creditLines,
  monthlyFixedCost: c.monthlyFixedCost,
  netDebt: c.netDebt,
  ebitda: c.ebitda,
  usdDebt: c.usdDebt,
  referenceDate: c.referenceDate,
});

export const toContractCommodityInput = (c: Omit<AddCommodityCommand, 'context' | 'commodity'>) => ({
  capacity: c.capacity,
  unit: measurementUnitEnum.toContract(c.unit),
  priceReference: c.priceReference,
  currency: c.currency,
  sells: c.sells,
});
