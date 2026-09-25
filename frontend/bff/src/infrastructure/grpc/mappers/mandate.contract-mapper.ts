import type { MandateTermsInput } from '../../../application/mandates/commands/mandate-terms.input.js';
import type { ComplianceDto } from '../../../application/mandates/dtos/compliance.dto.js';
import type { MandateDto } from '../../../application/mandates/dtos/mandate.dto.js';
import type { MandateTermsDto } from '../../../application/mandates/dtos/mandate-terms.dto.js';
import { nullable, nullableNumber } from './common.contract-mapper.js';
import { commodityEnum, complianceStatusEnum, mandateStatusEnum, mandateTypeEnum, measurementUnitEnum } from './enum.contract-mapper.js';

export interface ContractCompliance {
  status: string;
  reason: string;
}

export interface ContractMandateTerms {
  title: string;
  criteria?: string;
  commodity: string;
  tenor?: string;
  quantity?: number;
  quantityUnit: string;
  price: { atMarket: boolean; target?: number; min?: number; max?: number; unit?: string } | null;
  windowStart?: string;
  windowEnd?: string;
}

export interface ContractMandate {
  id: string;
  policyId: string;
  policyCode: string;
  policyVersion: string;
  axisId: string;
  axisCode: string;
  axisTitle: string;
  type: string;
  terms: ContractMandateTerms;
  consumed: number;
  balance?: number;
  compliance: ContractCompliance;
  status: string;
  issuedBy: string;
  decidedBy?: string;
  decidedAt?: string;
  decisionNote?: string;
}

export const toComplianceDto = (c: ContractCompliance): ComplianceDto => ({
  status: complianceStatusEnum.fromContractRequired(c.status),
  reason: c.reason,
});

const toTermsDto = (t: ContractMandateTerms): MandateTermsDto => ({
  title: t.title,
  criteria: nullable(t.criteria),
  commodity: commodityEnum.fromContract(t.commodity),
  tenor: nullable(t.tenor),
  quantity: nullableNumber(t.quantity),
  quantityUnit: measurementUnitEnum.fromContractRequired(t.quantityUnit),
  price: {
    atMarket: t.price?.atMarket ?? false,
    target: nullableNumber(t.price?.target),
    min: nullableNumber(t.price?.min),
    max: nullableNumber(t.price?.max),
    unit: nullable(t.price?.unit),
  },
  windowStart: nullable(t.windowStart),
  windowEnd: nullable(t.windowEnd),
});

export const toMandateDto = (m: ContractMandate): MandateDto => ({
  id: m.id,
  policyId: m.policyId,
  policyCode: m.policyCode,
  policyVersion: m.policyVersion,
  axisId: m.axisId,
  axisCode: m.axisCode,
  axisTitle: m.axisTitle,
  type: mandateTypeEnum.fromContractRequired(m.type),
  terms: toTermsDto(m.terms),
  consumed: m.consumed,
  balance: nullableNumber(m.balance),
  compliance: toComplianceDto(m.compliance),
  status: mandateStatusEnum.fromContractRequired(m.status),
  issuedBy: m.issuedBy,
  decidedBy: nullable(m.decidedBy),
  decidedAt: nullable(m.decidedAt),
  decisionNote: nullable(m.decisionNote),
});

export const toContractMandateTerms = (t: MandateTermsInput) => ({
  ...t,
  commodity: commodityEnum.toContract(t.commodity),
  quantityUnit: measurementUnitEnum.toContract(t.quantityUnit),
  price: { ...t.price },
});
