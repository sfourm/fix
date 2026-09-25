import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface MarkOrderDivergentCommand {
  context: RequestContext;
  id: string;
  note: string;
}
