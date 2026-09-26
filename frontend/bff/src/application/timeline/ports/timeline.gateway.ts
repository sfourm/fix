import type { Page } from '../../../cross-cutting/paging/page.js';
import type { TimelineEntryDto } from '../dtos/timeline-entry.dto.js';
import type { GetTimelinePageQuery, GetTimelineQuery } from '../queries/get-timeline.query.js';

export interface TimelineGateway {
  list(query: GetTimelineQuery): Promise<TimelineEntryDto[]>;
  page(query: GetTimelinePageQuery): Promise<Page<TimelineEntryDto>>;
}
