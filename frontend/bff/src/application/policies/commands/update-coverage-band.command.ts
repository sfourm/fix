import type { CoverageBandInput } from './coverage-band.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateCoverageBandCommand {
  context: RequestContext;
  policyId: string;
  bandId: string;
  band: CoverageBandInput;
}
