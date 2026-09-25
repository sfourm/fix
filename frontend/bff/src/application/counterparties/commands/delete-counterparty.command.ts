import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface DeleteCounterpartyCommand {
  context: RequestContext;
  id: string;
}
