import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ApproveOrderCommand {
  context: RequestContext;
  id: string;
  note: string | null;
}
