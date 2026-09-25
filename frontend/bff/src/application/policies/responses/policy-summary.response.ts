import type { PolicyStatus } from '../../../cross-cutting/enums/policy-status.js';

export interface PolicySummaryResponse {
  id: string;
  code: string;
  title: string;
  version: string;
  status: PolicyStatus;
  validFrom: string;
  validTo: string | null;
  axesCount: number;
}
