import type { Desk } from '../../../cross-cutting/enums/desk.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ChangeMemberDeskCommand {
  context: RequestContext;
  memberId: string;
  desk: Desk | null;
}
