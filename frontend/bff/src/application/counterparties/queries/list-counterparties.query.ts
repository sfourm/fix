import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListCounterpartiesQuery {
  context: RequestContext;
  onlyHomologated: boolean;
}
