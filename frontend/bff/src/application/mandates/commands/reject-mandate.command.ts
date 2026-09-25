import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RejectMandateCommand {
  context: RequestContext;
  id: string;
  /** Justificativa obrigatória. */
  note: string;
}
