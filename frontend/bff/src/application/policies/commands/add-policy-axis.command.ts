import type { PolicyAxisInput } from './policy-axis.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddPolicyAxisCommand {
  context: RequestContext;
  policyId: string;
  axis: PolicyAxisInput;
}
