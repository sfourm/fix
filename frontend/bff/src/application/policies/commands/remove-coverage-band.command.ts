import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RemoveCoverageBandCommand {
  context: RequestContext;
  policyId: string;
  bandId: string;
}
