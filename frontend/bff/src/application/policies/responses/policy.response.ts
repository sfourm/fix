import type { CoverageBandResponse } from './coverage-band.response.js';
import type { PolicyAxisResponse } from './policy-axis.response.js';
import type { PolicyInstrumentResponse } from './policy-instrument.response.js';
import type { PolicyLimitsResponse } from './policy-limits.response.js';
import type { PolicyStatus } from '../../../cross-cutting/enums/policy-status.js';
import type { PolicyVersionResponse } from './policy-version.response.js';

export interface PolicyResponse {
  id: string;
  code: string;
  title: string;
  version: string;
  description: string | null;
  status: PolicyStatus;
  validFrom: string;
  validTo: string | null;
  approvalRecord: string | null;
  approvedOn: string | null;
  limits: PolicyLimitsResponse;
  axes: PolicyAxisResponse[];
  bands: CoverageBandResponse[];
  instruments: PolicyInstrumentResponse[];
  versions: PolicyVersionResponse[];
}
