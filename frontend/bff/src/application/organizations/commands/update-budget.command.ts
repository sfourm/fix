import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateBudgetCommand {
  context: RequestContext;
  /** Custo caixa · gatilho de fixação (¢/lb). */
  cashCost: number | null;
  /** Piso econômico (¢/lb). */
  economicFloor: number | null;
  equivalentPrice: number | null;
  targetMarginPct: number | null;
}
