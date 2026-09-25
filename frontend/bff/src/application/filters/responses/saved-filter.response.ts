import type { DataSource } from '../../../domain/common/data-source.js';
import type { Visibility } from '../../../domain/common/visibility.js';
import type { FilterCriterion } from '../../../domain/filters/filter-criterion.js';

export interface SavedFilterResponse {
  id: string;
  name: string;
  source: DataSource;
  criteria: FilterCriterion[];
  visibility: Visibility;
  ownerId: string;
  /** O usuário é o dono. */
  isMine: boolean;
  /** O usuário pode alterar (dono, ou gestor da organização em filtro público). */
  canEdit: boolean;
  createdAt: string;
  updatedAt: string;
}
