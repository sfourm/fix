import type { Actor } from '../../../domain/common/actor.js';
import type { SavedFilter } from '../../../domain/filters/saved-filter.js';
import type { SavedFilterResponse } from '../responses/saved-filter.response.js';

export const toSavedFilterResponse = (filter: SavedFilter, actor: Actor): SavedFilterResponse => ({
  id: filter.id,
  name: filter.name,
  source: filter.source,
  criteria: filter.criteria.map((c) => ({ ...c })),
  visibility: filter.visibility,
  ownerId: filter.ownerId,
  isMine: filter.isOwnedBy(actor.userId),
  canEdit: filter.canEdit(actor),
  createdAt: filter.createdAt.toISOString(),
  updatedAt: filter.updatedAt.toISOString(),
});
