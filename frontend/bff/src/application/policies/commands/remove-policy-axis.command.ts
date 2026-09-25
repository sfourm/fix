import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RemovePolicyAxisCommand {
  context: RequestContext;
  policyId: string;
  axisId: string;
}
