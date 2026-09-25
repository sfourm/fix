import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetTimelineQuery {
  context: RequestContext;
  /** Ex.: Policy, Mandate, Order, OrganizationMember. */
  entityType?: string | undefined;
  entityId?: string | undefined;
  limit?: number | undefined;
}
