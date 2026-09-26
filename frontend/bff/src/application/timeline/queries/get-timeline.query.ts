import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { PageRequest } from '../../../cross-cutting/paging/page.js';

/** Filtros da auditoria (todos opcionais). Período em ISO 8601: de (inclusive) até (exclusive). */
export interface TimelineFilters {
  /** Ex.: Policy, Mandate, Order, OrganizationMember. */
  entityType?: string | undefined;
  entityId?: string | undefined;
  /** Created | Updated | Deleted */
  action?: string | undefined;
  authorId?: string | undefined;
  from?: string | undefined;
  to?: string | undefined;
  /** Texto nos valores alterados ou no id da entidade. */
  search?: string | undefined;
}

/** Os mais recentes, até `limit`. */
export interface GetTimelineQuery extends TimelineFilters {
  context: RequestContext;
  limit?: number | undefined;
}

/** Paginado, com o total que atende aos filtros. */
export interface GetTimelinePageQuery extends TimelineFilters {
  context: RequestContext;
  page: PageRequest;
}
