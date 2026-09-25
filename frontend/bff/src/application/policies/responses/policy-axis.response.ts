import type { RiskFactor } from '../../../cross-cutting/enums/risk-factor.js';

export interface PolicyAxisResponse {
  id: string;
  code: string;
  title: string;
  factor: RiskFactor;
  statement: string | null;
  limitDescription: string | null;
  approver: string | null;
  restrictions: string[];
}
