import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface DeletePolicyCommand {
  context: RequestContext;
  id: string;
}
