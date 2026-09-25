import { mapPage, type Page } from '../../../cross-cutting/paging/page.js';
import type { ApproveMandateCommand } from '../commands/approve-mandate.command.js';
import type { CloseMandateCommand } from '../commands/close-mandate.command.js';
import type { DeleteMandateCommand } from '../commands/delete-mandate.command.js';
import type { IssueMandateCommand } from '../commands/issue-mandate.command.js';
import type { RejectMandateCommand } from '../commands/reject-mandate.command.js';
import type { UpdateMandateCommand } from '../commands/update-mandate.command.js';
import { toComplianceResponse, toMandateResponse } from '../mappers/mandate.mapper.js';
import type { MandateGateway } from '../ports/mandate.gateway.js';
import type { GetMandateQuery } from '../queries/get-mandate.query.js';
import type { ListMandatesQuery } from '../queries/list-mandates.query.js';
import type { PreviewMandateComplianceQuery } from '../queries/preview-mandate-compliance.query.js';
import type { ComplianceResponse } from '../responses/compliance.response.js';
import type { MandateResponse } from '../responses/mandate.response.js';

/** Mandatos: emissão com checagem de aderência à política, fila de aprovação e saldo. */
export class MandateService {
  constructor(private readonly gateway: MandateGateway) {}

  async issue(command: IssueMandateCommand): Promise<MandateResponse> {
    return toMandateResponse(await this.gateway.issue(command));
  }

  async update(command: UpdateMandateCommand): Promise<MandateResponse> {
    return toMandateResponse(await this.gateway.update(command));
  }

  async approve(command: ApproveMandateCommand): Promise<MandateResponse> {
    return toMandateResponse(await this.gateway.approve(command));
  }

  async reject(command: RejectMandateCommand): Promise<MandateResponse> {
    return toMandateResponse(await this.gateway.reject(command));
  }

  async close(command: CloseMandateCommand): Promise<MandateResponse> {
    return toMandateResponse(await this.gateway.close(command));
  }

  async delete(command: DeleteMandateCommand): Promise<void> {
    await this.gateway.delete(command);
  }

  async get(query: GetMandateQuery): Promise<MandateResponse> {
    return toMandateResponse(await this.gateway.get(query));
  }

  async list(query: ListMandatesQuery): Promise<Page<MandateResponse>> {
    return mapPage(await this.gateway.list(query), toMandateResponse);
  }

  async previewCompliance(query: PreviewMandateComplianceQuery): Promise<ComplianceResponse> {
    return toComplianceResponse(await this.gateway.previewCompliance(query));
  }
}
