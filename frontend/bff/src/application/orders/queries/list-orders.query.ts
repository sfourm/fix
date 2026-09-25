import type { ApprovalStatus } from '../../../cross-cutting/enums/approval-status.js';
import type { ConfirmationStatus } from '../../../cross-cutting/enums/confirmation-status.js';
import type { PageRequest } from '../../../cross-cutting/paging/page.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListOrdersQuery {
  context: RequestContext;
  mandateId: string | null;
  approval: ApprovalStatus | null;
  confirmation: ConfirmationStatus | null;
  page: PageRequest;
}
