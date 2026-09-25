import type { MandateTermsInput } from '../commands/mandate-terms.input.js';
import type { MandateType } from '../../../cross-cutting/enums/mandate-type.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface PreviewMandateComplianceQuery {
  context: RequestContext;
  policyId: string;
  axisId: string;
  type: MandateType;
  terms: MandateTermsInput;
}
