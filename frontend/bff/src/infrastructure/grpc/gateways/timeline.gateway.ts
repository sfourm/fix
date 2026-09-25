import type { TimelineEntryDto } from '../../../application/timeline/dtos/timeline-entry.dto.js';
import type { TimelineGateway } from '../../../application/timeline/ports/timeline.gateway.js';
import type { GetTimelineQuery } from '../../../application/timeline/queries/get-timeline.query.js';
import type { CoreClient } from '../core-client.js';
import { optional, toContractContext } from '../mappers/common.contract-mapper.js';
import { toTimelineEntryDto, type ContractTimelineEntry } from '../mappers/timeline-entry.contract-mapper.js';

export class GrpcTimelineGateway implements TimelineGateway {
  constructor(private readonly core: CoreClient) {}

  async list(query: GetTimelineQuery): Promise<TimelineEntryDto[]> {
    const response = await this.core.call<{ entries: ContractTimelineEntry[] }>('TimelineService', 'GetTimeline', {
      context: toContractContext(query.context),
      limit: query.limit ?? 0,
      ...optional('entityType', query.entityType),
      ...optional('entityId', query.entityId),
    });
    return response.entries.map(toTimelineEntryDto);
  }
}
