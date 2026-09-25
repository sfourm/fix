import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface CreatePolicyCommand {
  context: RequestContext;
  code: string;
  title: string;
  version: string;
  description: string | null;
  validFrom: string;
  validTo: string | null;
  /** Inicia com os eixos, instrumentos e bandas do modelo FIX. */
  useTemplate: boolean;
}
