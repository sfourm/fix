import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface DeleteSavedFilterCommand {
  context: RequestContext;
  id: string;
}
