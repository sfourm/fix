import type { TimelineEntryDto } from '../../../application/timeline/dtos/timeline-entry.dto.js';
import { nullable, toIso, type ContractTimestamp } from './common.contract-mapper.js';

export interface ContractTimelineEntry {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  changesJson: string;
  authorId?: string;
  occurredAt: ContractTimestamp | null;
}

export const toTimelineEntryDto = (e: ContractTimelineEntry): TimelineEntryDto => ({
  id: e.id,
  entityType: e.entityType,
  entityId: e.entityId,
  action: e.action,
  changesJson: e.changesJson,
  authorId: nullable(e.authorId),
  occurredAt: toIso(e.occurredAt),
});
