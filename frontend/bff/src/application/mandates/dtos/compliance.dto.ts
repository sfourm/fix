import type { ComplianceStatus } from '../../../cross-cutting/enums/compliance-status.js';

export interface ComplianceDto {
  status: ComplianceStatus;
  reason: string;
}
