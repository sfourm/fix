export interface TimelineEntry {
  id: string;
  entityType: string;
  entityId: string;
  action: 'Created' | 'Updated' | 'Deleted';
  changes: Record<string, unknown>;
  authorId: string | null;
  occurredAt: string;
}
