import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetCounterpartyQuery {
  context: RequestContext;
  id: string;
}
