import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Cria uma alçada personalizada da organização com as roles escolhidas. */
export interface CreateRuleCommand {
  context: RequestContext;
  name: string;
  roleCodes: string[];
}
