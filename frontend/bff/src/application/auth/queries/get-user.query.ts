import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetUserQuery {
  context: RequestContext;
}
