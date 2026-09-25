import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Exclui a alçada, retirando-a de todos os membros e grupos. */
export interface DeleteRuleCommand {
  context: RequestContext;
  id: string;
}
