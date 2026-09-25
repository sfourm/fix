import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateFinancialsCommand {
  context: RequestContext;
  cash: number | null;
  creditLines: number | null;
  monthlyFixedCost: number | null;
  netDebt: number | null;
  ebitda: number | null;
  usdDebt: number | null;
  referenceDate: string | null;
}
