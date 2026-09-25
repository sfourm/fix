import type { PolicyLimitsInput } from './policy-limits.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdatePolicyLimitsCommand {
  context: RequestContext;
  id: string;
  limits: PolicyLimitsInput;
}
