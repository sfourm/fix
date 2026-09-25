import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RemovePolicyInstrumentCommand {
  context: RequestContext;
  policyId: string;
  instrumentId: string;
}
