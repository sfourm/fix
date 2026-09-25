import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface DeleteOrderCommand {
  context: RequestContext;
  id: string;
}
