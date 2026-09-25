import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RefuseOrderConfirmationCommand {
  context: RequestContext;
  id: string;
  note: string;
}
