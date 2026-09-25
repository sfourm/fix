import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateIndustrialProfileCommand {
  context: RequestContext;
  millingCapacity: number | null;
  mixMinPct: number | null;
  mixMaxPct: number | null;
  mixGuidancePct: number | null;
}
