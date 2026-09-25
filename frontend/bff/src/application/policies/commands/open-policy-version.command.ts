import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface OpenPolicyVersionCommand {
  context: RequestContext;
  id: string;
  version: string;
  reason: string | null;
}
