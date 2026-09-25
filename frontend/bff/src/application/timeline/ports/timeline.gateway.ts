import type { TimelineEntryDto } from '../dtos/timeline-entry.dto.js';
import type { GetTimelineQuery } from '../queries/get-timeline.query.js';

export interface TimelineGateway {
  list(query: GetTimelineQuery): Promise<TimelineEntryDto[]>;
}
