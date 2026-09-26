import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetFileQuery {
  context: RequestContext;
  id: string;
}
