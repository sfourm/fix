import type { PolicyStatus } from '../../../cross-cutting/enums/policy-status.js';

export interface PolicyVersionResponse {
  version: string;
  status: PolicyStatus;
  date: string;
  note: string | null;
}
