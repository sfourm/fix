import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Substitui as alçadas atribuídas diretamente ao membro (a base owner/user não muda). */
export interface SetMemberRulesCommand {
  context: RequestContext;
  memberId: string;
  ruleCodes: string[];
}
