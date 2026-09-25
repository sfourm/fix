import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddGroupMemberCommand {
  context: RequestContext;
  groupId: string;
  memberId: string;
}
