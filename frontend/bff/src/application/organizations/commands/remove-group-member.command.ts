import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Tira o membro do grupo (ele continua na organização). */
export interface RemoveGroupMemberCommand {
  context: RequestContext;
  groupId: string;
  memberId: string;
}
