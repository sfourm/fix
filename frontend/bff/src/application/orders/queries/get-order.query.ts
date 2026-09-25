import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetOrderQuery {
  context: RequestContext;
  id: string;
}
