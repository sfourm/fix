import type { PolicyInstrumentInput } from './policy-instrument.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddPolicyInstrumentCommand {
  context: RequestContext;
  policyId: string;
  instrument: PolicyInstrumentInput;
}
