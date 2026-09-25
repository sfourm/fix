import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface SubmitPolicyCommand {
  context: RequestContext;
  id: string;
}
