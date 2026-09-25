import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetSavedFilterQuery {
  context: RequestContext;
  id: string;
}
