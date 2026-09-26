import type { TimelineEntryDto } from '../../../application/timeline/dtos/timeline-entry.dto.js';
import type { TimelineGateway } from '../../../application/timeline/ports/timeline.gateway.js';
import type { GetTimelinePageQuery, GetTimelineQuery, TimelineFilters } from '../../../application/timeline/queries/get-timeline.query.js';
import type { Page } from '../../../cross-cutting/paging/page.js';
import type { CoreClient } from '../core-client.js';
import { optional, toContractContext, toContractPage, toPage, type ContractPageInfo } from '../mappers/common.contract-mapper.js';
import { toTimelineEntryDto, type ContractTimelineEntry } from '../mappers/timeline-entry.contract-mapper.js';

type TimelineResponse = { entries: ContractTimelineEntry[]; page: ContractPageInfo | null };

const toContractFilters = (filters: TimelineFilters) => ({
  ...optional('entityType', filters.entityType),
  ...optional('entityId', filters.entityId),
  ...optional('action', filters.action),
  ...optional('authorId', filters.authorId),
  ...optional('occurredFrom', filters.from),
  ...optional('occurredTo', filters.to),
  ...optional('search', filters.search),
});

export class GrpcTimelineGateway implements TimelineGateway {
  constructor(private readonly core: CoreClient) {}

  async list({ context, limit, ...filters }: GetTimelineQuery): Promise<TimelineEntryDto[]> {
    const response = await this.core.call<TimelineResponse>('TimelineService', 'GetTimeline', {
      context: toContractContext(context),
      limit: limit ?? 0,
      ...toContractFilters(filters),
    });
    return response.entries.map(toTimelineEntryDto);
  }

  async page({ context, page, ...filters }: GetTimelinePageQuery): Promise<Page<TimelineEntryDto>> {
    const response = await this.core.call<TimelineResponse>('TimelineService', 'GetTimeline', {
      context: toContractContext(context),
      page: toContractPage(page),
      ...toContractFilters(filters),
    });
    return toPage(response.entries, response.page, toTimelineEntryDto);
  }
}
