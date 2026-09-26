import { mapPage, type Page } from '../../../cross-cutting/paging/page.js';
import { toTimelineEntryResponse } from '../mappers/timeline-entry.mapper.js';
import type { TimelineGateway } from '../ports/timeline.gateway.js';
import type { GetTimelinePageQuery, GetTimelineQuery } from '../queries/get-timeline.query.js';
import type { TimelineEntryResponse } from '../responses/timeline-entry.response.js';

export class TimelineService {
  constructor(private readonly gateway: TimelineGateway) {}

  async list(query: GetTimelineQuery): Promise<TimelineEntryResponse[]> {
    return (await this.gateway.list(query)).map(toTimelineEntryResponse);
  }

  async page(query: GetTimelinePageQuery): Promise<Page<TimelineEntryResponse>> {
    return mapPage(await this.gateway.page(query), toTimelineEntryResponse);
  }
}
