import type { MandateStatus } from '../../../cross-cutting/enums/mandate-status.js';
import type { PageRequest } from '../../../cross-cutting/paging/page.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListMandatesQuery {
  context: RequestContext;
  policyId: string | null;
  status: MandateStatus | null;
  page: PageRequest;
}
