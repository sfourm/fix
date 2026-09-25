import type { ComplianceResponse } from './compliance.response.js';
import type { MandateStatus } from '../../../cross-cutting/enums/mandate-status.js';
import type { MandateTermsResponse } from './mandate-terms.response.js';
import type { MandateType } from '../../../cross-cutting/enums/mandate-type.js';

export interface MandateResponse {
  id: string;
  policyId: string;
  policyCode: string;
  policyVersion: string;
  axisId: string;
  axisCode: string;
  axisTitle: string;
  type: MandateType;
  terms: MandateTermsResponse;
  /** Consumido por boletas aprovadas (lotes ou US$). */
  consumed: number;
  /** Saldo; null = sem teto de volume. */
  balance: number | null;
  compliance: ComplianceResponse;
  status: MandateStatus;
  issuedBy: string;
  decidedBy: string | null;
  decidedAt: string | null;
  decisionNote: string | null;
}
