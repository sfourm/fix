import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetMandateQuery {
  context: RequestContext;
  id: string;
}
