import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Vínculo a posteriori: boleta sem mandato ligada a um mandato ativo, com justificativa (carimbo permanente). */
export interface LinkOrderMandateCommand {
  context: RequestContext;
  id: string;
  mandateId: string;
  justification: string;
}
