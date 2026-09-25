import type { Page } from '../../../cross-cutting/paging/page.js';
import type { ApproveMandateCommand } from '../commands/approve-mandate.command.js';
import type { CloseMandateCommand } from '../commands/close-mandate.command.js';
import type { DeleteMandateCommand } from '../commands/delete-mandate.command.js';
import type { IssueMandateCommand } from '../commands/issue-mandate.command.js';
import type { RejectMandateCommand } from '../commands/reject-mandate.command.js';
import type { UpdateMandateCommand } from '../commands/update-mandate.command.js';
import type { ComplianceDto } from '../dtos/compliance.dto.js';
import type { MandateDto } from '../dtos/mandate.dto.js';
import type { GetMandateQuery } from '../queries/get-mandate.query.js';
import type { ListMandatesQuery } from '../queries/list-mandates.query.js';
import type { PreviewMandateComplianceQuery } from '../queries/preview-mandate-compliance.query.js';

export interface MandateGateway {
  issue(command: IssueMandateCommand): Promise<MandateDto>;
  update(command: UpdateMandateCommand): Promise<MandateDto>;
  approve(command: ApproveMandateCommand): Promise<MandateDto>;
  reject(command: RejectMandateCommand): Promise<MandateDto>;
  close(command: CloseMandateCommand): Promise<MandateDto>;
  delete(command: DeleteMandateCommand): Promise<void>;
  get(query: GetMandateQuery): Promise<MandateDto>;
  list(query: ListMandatesQuery): Promise<Page<MandateDto>>;
  previewCompliance(query: PreviewMandateComplianceQuery): Promise<ComplianceDto>;
}
