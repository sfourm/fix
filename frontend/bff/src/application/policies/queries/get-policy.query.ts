import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetPolicyQuery {
  context: RequestContext;
  id: string;
}
