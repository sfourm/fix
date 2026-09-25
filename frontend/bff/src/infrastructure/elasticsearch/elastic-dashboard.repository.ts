import type { Client } from '@elastic/elasticsearch';
import type { Visibility } from '../../domain/common/visibility.js';
import { Dashboard } from '../../domain/dashboards/dashboard.js';
import type { DashboardRepository } from '../../domain/dashboards/dashboard.repository.js';
import type { DashboardWidget } from '../../domain/dashboards/dashboard-widget.js';
import { MAX_RESULTS, visibleTo } from './visibility-query.js';

/** Documento no índice: o estado do agregado + filterIds derivado (para achar dashboards que usam um filtro). */
interface DashboardDocument {
  id: string;
  organizationId: string;
  ownerId: string;
  name: string;
  description: string | null;
  visibility: Visibility;
  widgets: DashboardWidget[];
  filterIds: string[];
  createdAt: string;
  updatedAt: string;
}

export class ElasticDashboardRepository implements DashboardRepository {
  constructor(
    private readonly client: Client,
    private readonly index: string,
  ) {}

  async findById(organizationId: string, id: string): Promise<Dashboard | null> {
    const response = await this.client.search<DashboardDocument>({
      index: this.index,
      size: 1,
      query: { bool: { filter: [{ term: { organizationId } }, { ids: { values: [id] } }] } },
    });
    const source = response.hits.hits[0]?._source;
    return source ? toEntity(source) : null;
  }

  async listVisible(organizationId: string, userId: string): Promise<Dashboard[]> {
    const response = await this.client.search<DashboardDocument>({
      index: this.index,
      size: MAX_RESULTS,
      query: visibleTo(organizationId, userId),
      sort: [{ 'name.sort': 'asc' }],
    });
    return response.hits.hits.flatMap((hit) => (hit._source ? [toEntity(hit._source)] : []));
  }

  async countUsingFilter(organizationId: string, filterId: string, options: { onlyPublic?: boolean } = {}): Promise<number> {
    const response = await this.client.count({
      index: this.index,
      query: {
        bool: {
          filter: [
            { term: { organizationId } },
            { term: { filterIds: filterId } },
            ...(options.onlyPublic ? [{ term: { visibility: 'Public' } }] : []),
          ],
        },
      },
    });
    return response.count;
  }

  async save(dashboard: Dashboard): Promise<void> {
    await this.client.index({ index: this.index, id: dashboard.id, document: toDocument(dashboard), refresh: 'wait_for' });
  }

  async delete(organizationId: string, id: string): Promise<void> {
    await this.client.deleteByQuery({
      index: this.index,
      refresh: true,
      query: { bool: { filter: [{ term: { organizationId } }, { ids: { values: [id] } }] } },
    });
  }
}

function toDocument(dashboard: Dashboard): DashboardDocument {
  const s = dashboard.toSnapshot();
  return {
    id: s.id,
    organizationId: s.organizationId,
    ownerId: s.ownerId,
    name: s.name,
    description: s.description,
    visibility: s.visibility,
    widgets: s.widgets,
    filterIds: [...new Set(s.widgets.map((w) => w.query.filterId).filter((id): id is string => !!id))],
    createdAt: s.createdAt.toISOString(),
    updatedAt: s.updatedAt.toISOString(),
  };
}

function toEntity(doc: DashboardDocument): Dashboard {
  return Dashboard.restore({
    id: doc.id,
    organizationId: doc.organizationId,
    ownerId: doc.ownerId,
    name: doc.name,
    description: doc.description,
    visibility: doc.visibility,
    widgets: doc.widgets,
    createdAt: new Date(doc.createdAt),
    updatedAt: new Date(doc.updatedAt),
  });
}
