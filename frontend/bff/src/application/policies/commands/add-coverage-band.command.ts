import type { CoverageBandInput } from './coverage-band.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddCoverageBandCommand {
  context: RequestContext;
  policyId: string;
  band: CoverageBandInput;
}
