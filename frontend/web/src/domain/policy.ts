export const POLICY_STATUSES = ['Draft', 'UnderApproval', 'Active', 'Superseded'] as const;
export type PolicyStatus = (typeof POLICY_STATUSES)[number];

/** Fator de risco coberto por um eixo da política. */
export const RISK_FACTORS = ['Physical', 'Price', 'Currency', 'Freight'] as const;
export type RiskFactor = (typeof RISK_FACTORS)[number];

export const INSTRUMENT_PERMISSIONS = ['Allowed', 'Capped', 'Forbidden'] as const;
export type InstrumentPermission = (typeof INSTRUMENT_PERMISSIONS)[number];

/** Parâmetros quantitativos da política-mãe (percentuais em %). */
export interface PolicyLimits {
  hedgeHorizonYears: number;
  absoluteCeilingPct: number;
  fxFixedMinPct: number;
  fxFixedMaxPct: number;
  fxUnfixedMaxPct: number;
  marginCashMaxPct: number;
  physicalConcentrationMaxPct: number;
  financialConcentrationMaxPct: number;
  logisticsDeadlineMonths: number;
  freightCeilingPct: number;
  coveredCallMaxPct: number;
  /** Contingência escalonada (% da produção projetada que nunca é vendida nem fixada, por prazo). */
  contingency1MonthPct: number;
  contingency6MonthsPct: number;
  contingency12MonthsPct: number;
  contingency24MonthsPct: number;
  contingency36MonthsPct: number;
  buybackTriggerPct: number;
  buybackDeadlineBusinessDays: number;
  stressSigmas: number;
  stressDays: number;
  pricingHotPercentile: number;
  pricingColdPercentile: number;
  mixShiftMaxPp: number;
  confirmationDeadlineBusinessDays: number;
  registrationDeadlineDays: number;
  deviationReportHours: number;
}

export interface PolicyAxis {
  id: string;
  code: string;
  title: string;
  factor: RiskFactor;
  statement: string | null;
  limitDescription: string | null;
  approver: string | null;
  restrictions: string[];
}

export interface CoverageBand {
  id: string;
  horizon: string;
  /** AA/AA */
  crop: string;
  minPct: number;
  maxPct: number;
  note: string | null;
}

export interface PolicyInstrument {
  id: string;
  name: string;
  permission: InstrumentPermission;
  condition: string | null;
}

export interface PolicyVersion {
  version: string;
  status: PolicyStatus;
  date: string;
  note: string | null;
}

export interface PolicySummary {
  id: string;
  code: string;
  title: string;
  version: string;
  status: PolicyStatus;
  validFrom: string;
  validTo: string | null;
  axesCount: number;
}

export interface Policy extends Omit<PolicySummary, 'axesCount'> {
  description: string | null;
  approvalRecord: string | null;
  approvedOn: string | null;
  limits: PolicyLimits;
  axes: PolicyAxis[];
  bands: CoverageBand[];
  instruments: PolicyInstrument[];
  versions: PolicyVersion[];
}

export type PolicyAxisInput = Omit<PolicyAxis, 'id'>;
export type CoverageBandInput = Omit<CoverageBand, 'id'>;
export type PolicyInstrumentInput = Omit<PolicyInstrument, 'id'>;

/** Só rascunho e política em aprovação aceitam edição de parâmetros, eixos, bandas e instrumentos. */
export const isPolicyEditable = (policy: Pick<Policy, 'status'>) => policy.status === 'Draft' || policy.status === 'UnderApproval';
