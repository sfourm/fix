import type { Desk } from '../../../cross-cutting/enums/desk.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddMemberCommand {
  context: RequestContext;
  email: string;
  ruleCode: string;
  desk: Desk | null;
}
