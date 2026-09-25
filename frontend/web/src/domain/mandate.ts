import type { Commodity, MeasurementUnit } from './common';
import type { RiskFactor } from './policy';

export const MANDATE_TYPES = ['Pricing', 'Currency', 'Commercial', 'Logistics'] as const;
export type MandateType = (typeof MANDATE_TYPES)[number];

export const MANDATE_STATUSES = ['PendingApproval', 'Active', 'Rejected', 'Closed'] as const;
export type MandateStatus = (typeof MANDATE_STATUSES)[number];

export const COMPLIANCE_STATUSES = ['Within', 'Outside'] as const;
export type ComplianceStatus = (typeof COMPLIANCE_STATUSES)[number];

/** Cada tipo de mandato só pode ser emitido sobre um eixo do fator de risco correspondente. */
export const mandateTypeFactor: Record<MandateType, RiskFactor> = {
  Pricing: 'Price',
  Currency: 'Currency',
  Commercial: 'Physical',
  Logistics: 'Freight',
};

export interface Compliance {
  status: ComplianceStatus;
  reason: string;
}

export interface PriceCriteria {
  /** A mercado (sem preço-alvo). */
  atMarket: boolean;
  target: number | null;
  min: number | null;
  max: number | null;
  /** Ex.: c/lb, R$/t, R$/US$. */
  unit: string | null;
}

export interface MandateTerms {
  title: string;
  criteria: string | null;
  /** Ausente em NDF / em mandatos que não são de precificação. */
  commodity: Commodity | null;
  /** Tela/vencimento: N26, fev/27. */
  tenor: string | null;
  quantity: number | null;
  quantityUnit: MeasurementUnit;
  price: PriceCriteria;
  windowStart: string | null;
  windowEnd: string | null;
}

export interface Mandate {
  id: string;
  policyId: string;
  policyCode: string;
  policyVersion: string;
  axisId: string;
  axisCode: string;
  axisTitle: string;
  type: MandateType;
  terms: MandateTerms;
  /** Consumido por boletas aprovadas (lotes ou US$). */
  consumed: number;
  /** Saldo; null = sem teto de volume. */
  balance: number | null;
  compliance: Compliance;
  status: MandateStatus;
  issuedBy: string;
  decidedBy: string | null;
  decidedAt: string | null;
  decisionNote: string | null;
}

export interface MandateIssueInput {
  policyId: string;
  axisId: string;
  type: MandateType;
  terms: MandateTerms;
}
