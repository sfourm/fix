export interface TimelineEntry {
  id: string;
  entityType: string;
  entityId: string;
  action: 'Created' | 'Updated' | 'Deleted';
  changes: Record<string, unknown>;
  authorId: string | null;
  occurredAt: string;
}

/** Filtros da auditoria (todos opcionais). Período em ISO 8601: de (inclusive) até (exclusive). */
export interface TimelineFilters {
  action?: TimelineEntry['action'];
  authorId?: string;
  from?: string;
  to?: string;
  search?: string;
}
