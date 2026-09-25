import type { PageRequest } from '../../../cross-cutting/paging/page.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListPoliciesQuery {
  context: RequestContext;
  page: PageRequest;
}
