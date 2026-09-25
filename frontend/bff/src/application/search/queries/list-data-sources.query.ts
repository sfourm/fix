import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListDataSourcesQuery {
  context: RequestContext;
}
