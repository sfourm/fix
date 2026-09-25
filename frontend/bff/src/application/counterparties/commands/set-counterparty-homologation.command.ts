import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface SetCounterpartyHomologationCommand {
  context: RequestContext;
  id: string;
  homologated: boolean;
}
