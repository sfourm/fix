import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdatePolicyCommand {
  context: RequestContext;
  id: string;
  code: string;
  title: string;
  description: string | null;
  validFrom: string;
  validTo: string | null;
}
