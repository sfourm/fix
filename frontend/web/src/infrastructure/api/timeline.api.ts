import type { Page } from '@/domain/page';
import type { TimelineEntry, TimelineFilters } from '@/domain/timeline';
import type { HttpClient } from '../http/http-client';

type Scope = { entityType?: string; entityId?: string } & TimelineFilters;

export function createTimelineApi(http: HttpClient) {
  return {
    /** Os mais recentes (até limit): históricos dentro das telas. */
    list: (query: Scope & { limit?: number }) => http.get<TimelineEntry[]>('/timeline', { ...query }),
    /** Paginado, com o total que atende aos filtros (tela de Auditoria e exportação). */
    page: (query: Scope & { page: number; pageSize: number }) => http.get<Page<TimelineEntry>>('/timeline/page', { ...query }),
  };
}
