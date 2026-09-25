import type { TimelineEntry } from '@/domain/timeline';
import type { HttpClient } from '../http/http-client';

export function createTimelineApi(http: HttpClient) {
  return {
    list: (query: { entityType?: string; entityId?: string; limit?: number }) =>
      http.get<TimelineEntry[]>('/timeline', { ...query }),
  };
}
