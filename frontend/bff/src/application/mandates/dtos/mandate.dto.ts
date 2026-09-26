import type { ComplianceDto } from './compliance.dto.js';
import type { MandateStatus } from '../../../cross-cutting/enums/mandate-status.js';
import type { MandateTermsDto } from './mandate-terms.dto.js';
import type { MandateType } from '../../../cross-cutting/enums/mandate-type.js';

export interface MandateDto {
  id: string;
  /** Código legível da organização (MD-01). */
  code: string;
  policyId: string;
  policyCode: string;
  policyVersion: string;
  axisId: string;
  axisCode: string;
  axisTitle: string;
  type: MandateType;
  terms: MandateTermsDto;
  /** Consumido por boletas aprovadas (lotes ou US$). */
  consumed: number;
  /** Saldo; null = sem teto de volume. */
  balance: number | null;
  compliance: ComplianceDto;
  status: MandateStatus;
  issuedBy: string;
  decidedBy: string | null;
  decidedAt: string | null;
  decisionNote: string | null;
}
