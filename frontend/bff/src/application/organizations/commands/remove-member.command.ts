import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RemoveMemberCommand {
  context: RequestContext;
  memberId: string;
}
