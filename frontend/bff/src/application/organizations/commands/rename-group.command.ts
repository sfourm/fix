import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Renomeia um grupo do organograma. */
export interface RenameGroupCommand {
  context: RequestContext;
  groupId: string;
  name: string;
}
