import type { MandateTermsInput } from './mandate-terms.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateMandateCommand {
  context: RequestContext;
  id: string;
  terms: MandateTermsInput;
}
