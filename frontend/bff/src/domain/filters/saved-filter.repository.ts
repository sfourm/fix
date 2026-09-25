import type { DataSource } from '../common/data-source.js';
import type { SavedFilter } from './saved-filter.js';

export interface SavedFilterRepository {
  findById(organizationId: string, id: string): Promise<SavedFilter | null>;
  /** Filtros visíveis ao usuário: os próprios e os públicos da organização. */
  listVisible(organizationId: string, userId: string, source?: DataSource): Promise<SavedFilter[]>;
  save(filter: SavedFilter): Promise<void>;
  delete(organizationId: string, id: string): Promise<void>;
}
