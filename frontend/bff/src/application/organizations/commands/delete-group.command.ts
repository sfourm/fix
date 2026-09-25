import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Exclui um grupo do organograma e as alçadas atribuídas a ele. */
export interface DeleteGroupCommand {
  context: RequestContext;
  groupId: string;
}
