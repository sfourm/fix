import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetDashboardQuery {
  context: RequestContext;
  id: string;
}
