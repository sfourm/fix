/** Entrada de auditoria como vem do core: as mudanças chegam serializadas em JSON. */
export interface TimelineEntryDto {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  changesJson: string;
  authorId: string | null;
  /** ISO 8601 */
  occurredAt: string;
}
