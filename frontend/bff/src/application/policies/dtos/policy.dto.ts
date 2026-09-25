import type { CoverageBandDto } from './coverage-band.dto.js';
import type { PolicyAxisDto } from './policy-axis.dto.js';
import type { PolicyInstrumentDto } from './policy-instrument.dto.js';
import type { PolicyLimitsDto } from './policy-limits.dto.js';
import type { PolicyStatus } from '../../../cross-cutting/enums/policy-status.js';
import type { PolicyVersionDto } from './policy-version.dto.js';

export interface PolicyDto {
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
  limits: PolicyLimitsDto;
  axes: PolicyAxisDto[];
  bands: CoverageBandDto[];
  instruments: PolicyInstrumentDto[];
  versions: PolicyVersionDto[];
}
