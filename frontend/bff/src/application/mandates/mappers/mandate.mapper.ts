import type { ComplianceDto } from '../dtos/compliance.dto.js';
import type { MandateDto } from '../dtos/mandate.dto.js';
import type { ComplianceResponse } from '../responses/compliance.response.js';
import type { MandateResponse } from '../responses/mandate.response.js';

export const toComplianceResponse = (dto: ComplianceDto): ComplianceResponse => ({ ...dto });

export const toMandateResponse = (dto: MandateDto): MandateResponse => ({
  ...dto,
  terms: { ...dto.terms, price: { ...dto.terms.price } },
  compliance: toComplianceResponse(dto.compliance),
});
