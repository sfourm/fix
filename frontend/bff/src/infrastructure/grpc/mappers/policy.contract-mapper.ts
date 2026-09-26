import type { CoverageBandInput } from '../../../application/policies/commands/coverage-band.input.js';
import type { PolicyAxisInput } from '../../../application/policies/commands/policy-axis.input.js';
import type { PolicyInstrumentInput } from '../../../application/policies/commands/policy-instrument.input.js';
import type { PolicyLimitsDto } from '../../../application/policies/dtos/policy-limits.dto.js';
import type { PolicySummaryDto } from '../../../application/policies/dtos/policy-summary.dto.js';
import type { PolicyDto } from '../../../application/policies/dtos/policy.dto.js';
import { nullable } from './common.contract-mapper.js';
import { instrumentPermissionEnum, policyStatusEnum, riskFactorEnum } from './enum.contract-mapper.js';

/** No contrato a contingência vem por extenso (contingencyOneMonthPct...); no DTO, com o prazo em número. */
export type ContractPolicyLimits = Omit<PolicyLimitsDto, 'contingency1MonthPct' | 'contingency6MonthsPct' | 'contingency12MonthsPct' | 'contingency24MonthsPct' | 'contingency36MonthsPct'> & {
  contingencyOneMonthPct: number;
  contingencySixMonthsPct: number;
  contingencyTwelveMonthsPct: number;
  contingencyTwentyFourMonthsPct: number;
  contingencyThirtySixMonthsPct: number;
};

const CONTINGENCY: [keyof PolicyLimitsDto, keyof ContractPolicyLimits][] = [
  ['contingency1MonthPct', 'contingencyOneMonthPct'],
  ['contingency6MonthsPct', 'contingencySixMonthsPct'],
  ['contingency12MonthsPct', 'contingencyTwelveMonthsPct'],
  ['contingency24MonthsPct', 'contingencyTwentyFourMonthsPct'],
  ['contingency36MonthsPct', 'contingencyThirtySixMonthsPct'],
];

export const fromContractLimits = (limits: Partial<ContractPolicyLimits> | null): PolicyLimitsDto => {
  const dto = { ...emptyLimits, ...(limits ?? {}) } as PolicyLimitsDto & Record<string, unknown>;
  for (const [key, contractKey] of CONTINGENCY) {
    dto[key] = Number(limits?.[contractKey] ?? 0) as never;
    delete dto[contractKey];
  }
  return dto;
};

export const toContractLimits = (limits: PolicyLimitsDto): ContractPolicyLimits => {
  const contract = { ...limits } as Record<string, unknown>;
  for (const [key, contractKey] of CONTINGENCY) {
    contract[contractKey] = limits[key];
    delete contract[key];
  }
  return contract as ContractPolicyLimits;
};

export interface ContractPolicyAxis {
  id: string;
  code: string;
  title: string;
  factor: string;
  statement?: string;
  limitDescription?: string;
  approver?: string;
  restrictions: string[];
}

export interface ContractCoverageBand {
  id: string;
  horizon: string;
  crop: string;
  minPct: number;
  maxPct: number;
  note?: string;
}

export interface ContractPolicyInstrument {
  id: string;
  name: string;
  permission: string;
  condition?: string;
}

export interface ContractPolicyVersion {
  version: string;
  status: string;
  date: string;
  note?: string;
}

export interface ContractPolicySummary {
  id: string;
  code: string;
  title: string;
  version: string;
  status: string;
  validFrom: string;
  validTo?: string;
  axesCount: number;
}

export interface ContractPolicy extends Omit<ContractPolicySummary, 'axesCount'> {
  description?: string;
  approvalRecord?: string;
  approvedOn?: string;
  limits: ContractPolicyLimits | null;
  axes: ContractPolicyAxis[];
  bands: ContractCoverageBand[];
  instruments: ContractPolicyInstrument[];
  versions: ContractPolicyVersion[];
}

const emptyLimits: PolicyLimitsDto = {
  hedgeHorizonYears: 0,
  absoluteCeilingPct: 0,
  fxFixedMinPct: 0,
  fxFixedMaxPct: 0,
  fxUnfixedMaxPct: 0,
  marginCashMaxPct: 0,
  physicalConcentrationMaxPct: 0,
  financialConcentrationMaxPct: 0,
  logisticsDeadlineMonths: 0,
  freightCeilingPct: 0,
  coveredCallMaxPct: 0,
  contingency1MonthPct: 0,
  contingency6MonthsPct: 0,
  contingency12MonthsPct: 0,
  contingency24MonthsPct: 0,
  contingency36MonthsPct: 0,
  buybackTriggerPct: 0,
  buybackDeadlineBusinessDays: 0,
  stressSigmas: 0,
  stressDays: 0,
  pricingHotPercentile: 0,
  pricingColdPercentile: 0,
  mixShiftMaxPp: 0,
  confirmationDeadlineBusinessDays: 0,
  registrationDeadlineDays: 0,
  deviationReportHours: 0,
};

export const toPolicySummaryDto = (p: ContractPolicySummary): PolicySummaryDto => ({
  id: p.id,
  code: p.code,
  title: p.title,
  version: p.version,
  status: policyStatusEnum.fromContractRequired(p.status),
  validFrom: p.validFrom,
  validTo: nullable(p.validTo),
  axesCount: p.axesCount,
});

export const toPolicyDto = (p: ContractPolicy): PolicyDto => ({
  id: p.id,
  code: p.code,
  title: p.title,
  version: p.version,
  description: nullable(p.description),
  status: policyStatusEnum.fromContractRequired(p.status),
  validFrom: p.validFrom,
  validTo: nullable(p.validTo),
  approvalRecord: nullable(p.approvalRecord),
  approvedOn: nullable(p.approvedOn),
  limits: fromContractLimits(p.limits),
  axes: p.axes.map((a) => ({
    id: a.id,
    code: a.code,
    title: a.title,
    factor: riskFactorEnum.fromContractRequired(a.factor),
    statement: nullable(a.statement),
    limitDescription: nullable(a.limitDescription),
    approver: nullable(a.approver),
    restrictions: a.restrictions,
  })),
  bands: p.bands.map((b) => ({
    id: b.id,
    horizon: b.horizon,
    crop: b.crop,
    minPct: b.minPct,
    maxPct: b.maxPct,
    note: nullable(b.note),
  })),
  instruments: p.instruments.map((i) => ({
    id: i.id,
    name: i.name,
    permission: instrumentPermissionEnum.fromContractRequired(i.permission),
    condition: nullable(i.condition),
  })),
  versions: p.versions.map((v) => ({
    version: v.version,
    status: policyStatusEnum.fromContractRequired(v.status),
    date: v.date,
    note: nullable(v.note),
  })),
});

export const toContractAxisInput = (a: PolicyAxisInput) => ({ ...a, factor: riskFactorEnum.toContract(a.factor) });

export const toContractBandInput = (b: CoverageBandInput) => ({ ...b });

export const toContractInstrumentInput = (i: PolicyInstrumentInput) => ({
  ...i,
  permission: instrumentPermissionEnum.toContract(i.permission),
});
