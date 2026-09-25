import type { Desk } from '../../../cross-cutting/enums/desk.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddMemberCommand {
  context: RequestContext;
  email: string;
  /** Alçada inicial (opcional): código de uma alçada da organização. */
  ruleCode: string | null;
  desk: Desk | null;
}
