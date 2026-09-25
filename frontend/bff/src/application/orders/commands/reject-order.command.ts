import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RejectOrderCommand {
  context: RequestContext;
  id: string;
  /** Justificativa obrigatória. */
  note: string;
}
