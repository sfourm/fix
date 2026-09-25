import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface CloseMandateCommand {
  context: RequestContext;
  id: string;
  note: string | null;
}
