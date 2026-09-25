import type { ComplianceStatus } from '../../../cross-cutting/enums/compliance-status.js';

export interface ComplianceResponse {
  status: ComplianceStatus;
  reason: string;
}
