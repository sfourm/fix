import type { PolicySummaryDto } from '../dtos/policy-summary.dto.js';
import type { PolicyDto } from '../dtos/policy.dto.js';
import type { PolicySummaryResponse } from '../responses/policy-summary.response.js';
import type { PolicyResponse } from '../responses/policy.response.js';

export const toPolicySummaryResponse = (dto: PolicySummaryDto): PolicySummaryResponse => ({ ...dto });

export const toPolicyResponse = (dto: PolicyDto): PolicyResponse => ({
  ...dto,
  limits: { ...dto.limits },
  axes: dto.axes.map((axis) => ({ ...axis, restrictions: [...axis.restrictions] })),
  bands: dto.bands.map((band) => ({ ...band })),
  instruments: dto.instruments.map((instrument) => ({ ...instrument })),
  versions: dto.versions.map((version) => ({ ...version })),
});
