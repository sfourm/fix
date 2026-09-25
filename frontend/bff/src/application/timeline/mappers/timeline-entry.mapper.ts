import type { TimelineEntryDto } from '../dtos/timeline-entry.dto.js';
import type { TimelineAction, TimelineEntryResponse } from '../responses/timeline-entry.response.js';

export const toTimelineEntryResponse = (dto: TimelineEntryDto): TimelineEntryResponse => ({
  id: dto.id,
  entityType: dto.entityType,
  entityId: dto.entityId,
  action: dto.action as TimelineAction,
  changes: parseChanges(dto.changesJson),
  authorId: dto.authorId,
  occurredAt: dto.occurredAt,
});

function parseChanges(json: string): Record<string, unknown> {
  try {
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return {};
  }
}
