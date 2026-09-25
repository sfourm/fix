import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Substitui as alçadas do grupo (todos os membros do grupo recebem as roles delas). */
export interface SetGroupRulesCommand {
  context: RequestContext;
  groupId: string;
  ruleCodes: string[];
}
