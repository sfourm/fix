import type { PolicyInstrumentInput } from './policy-instrument.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdatePolicyInstrumentCommand {
  context: RequestContext;
  policyId: string;
  instrumentId: string;
  instrument: PolicyInstrumentInput;
}
