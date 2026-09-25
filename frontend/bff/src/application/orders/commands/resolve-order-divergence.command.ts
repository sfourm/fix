import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ResolveOrderDivergenceCommand {
  context: RequestContext;
  id: string;
}
