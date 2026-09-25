import type { PolicyAxisInput } from './policy-axis.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdatePolicyAxisCommand {
  context: RequestContext;
  policyId: string;
  axisId: string;
  axis: PolicyAxisInput;
}
