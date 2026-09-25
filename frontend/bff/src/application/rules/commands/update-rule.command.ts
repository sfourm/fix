import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Renomeia a alçada e troca as roles concedidas (vale na hora para quem a tem). */
export interface UpdateRuleCommand {
  context: RequestContext;
  id: string;
  name: string;
  roleCodes: string[];
}
