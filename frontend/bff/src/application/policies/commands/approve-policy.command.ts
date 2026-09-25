import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ApprovePolicyCommand {
  context: RequestContext;
  id: string;
  /** Nº/data da ata do Conselho. */
  approvalRecord: string;
}
