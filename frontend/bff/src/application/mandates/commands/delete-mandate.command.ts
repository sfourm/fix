import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface DeleteMandateCommand {
  context: RequestContext;
  id: string;
}
