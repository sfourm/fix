import type { PolicyStatus } from '../../../cross-cutting/enums/policy-status.js';

export interface PolicyVersionDto {
  version: string;
  status: PolicyStatus;
  date: string;
  note: string | null;
}
