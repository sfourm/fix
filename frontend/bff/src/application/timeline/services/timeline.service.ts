import { toTimelineEntryResponse } from '../mappers/timeline-entry.mapper.js';
import type { TimelineGateway } from '../ports/timeline.gateway.js';
import type { GetTimelineQuery } from '../queries/get-timeline.query.js';
import type { TimelineEntryResponse } from '../responses/timeline-entry.response.js';

export class TimelineService {
  constructor(private readonly gateway: TimelineGateway) {}

  async list(query: GetTimelineQuery): Promise<TimelineEntryResponse[]> {
    return (await this.gateway.list(query)).map(toTimelineEntryResponse);
  }
}
