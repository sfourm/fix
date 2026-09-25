import type { Client } from '@elastic/elasticsearch';
import type { DataSource } from '../../domain/common/data-source.js';
import type { Visibility } from '../../domain/common/visibility.js';
import type { FilterCriterion } from '../../domain/filters/filter-criterion.js';
import { SavedFilter } from '../../domain/filters/saved-filter.js';
import type { SavedFilterRepository } from '../../domain/filters/saved-filter.repository.js';
import { MAX_RESULTS, visibleTo } from './visibility-query.js';

interface SavedFilterDocument {
  id: string;
  organizationId: string;
  ownerId: string;
  name: string;
  visibility: Visibility;
  source: DataSource;
  criteria: FilterCriterion[];
  createdAt: string;
  updatedAt: string;
}

export class ElasticSavedFilterRepository implements SavedFilterRepository {
  constructor(
    private readonly client: Client,
    private readonly index: string,
  ) {}

  async findById(organizationId: string, id: string): Promise<SavedFilter | null> {
    const response = await this.client.search<SavedFilterDocument>({
      index: this.index,
      size: 1,
      query: { bool: { filter: [{ term: { organizationId } }, { ids: { values: [id] } }] } },
    });
    const source = response.hits.hits[0]?._source;
    return source ? toEntity(source) : null;
  }

  async listVisible(organizationId: string, userId: string, source?: DataSource): Promise<SavedFilter[]> {
    const response = await this.client.search<SavedFilterDocument>({
      index: this.index,
      size: MAX_RESULTS,
      query: visibleTo(organizationId, userId, source ? [{ term: { source } }] : []),
      sort: [{ 'name.sort': 'asc' }],
    });
    return response.hits.hits.flatMap((hit) => (hit._source ? [toEntity(hit._source)] : []));
  }

  async save(filter: SavedFilter): Promise<void> {
    const s = filter.toSnapshot();
    const document: SavedFilterDocument = {
      ...s,
      criteria: [...s.criteria],
      createdAt: s.createdAt.toISOString(),
      updatedAt: s.updatedAt.toISOString(),
    };
    await this.client.index({ index: this.index, id: s.id, document, refresh: 'wait_for' });
  }

  async delete(organizationId: string, id: string): Promise<void> {
    await this.client.deleteByQuery({
      index: this.index,
      refresh: true,
      query: { bool: { filter: [{ term: { organizationId } }, { ids: { values: [id] } }] } },
    });
  }
}

const toEntity = (doc: SavedFilterDocument): SavedFilter =>
  SavedFilter.restore({ ...doc, createdAt: new Date(doc.createdAt), updatedAt: new Date(doc.updatedAt) });
