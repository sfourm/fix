export type TimelineAction = 'Created' | 'Updated' | 'Deleted';

export interface TimelineEntryResponse {
  id: string;
  entityType: string;
  entityId: string;
  action: TimelineAction;
  /** Valores (Created/Deleted) ou { campo: { old, new } } (Updated). */
  changes: Record<string, unknown>;
  authorId: string | null;
  occurredAt: string;
}
