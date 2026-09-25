import type { Dashboard } from './dashboard.js';

export interface DashboardRepository {
  findById(organizationId: string, id: string): Promise<Dashboard | null>;
  /** Dashboards visíveis ao usuário: os próprios e os públicos da organização. */
  listVisible(organizationId: string, userId: string): Promise<Dashboard[]>;
  /** Dashboards que usam o filtro salvo (para impedir excluir um filtro em uso). */
  countUsingFilter(organizationId: string, filterId: string, options?: { onlyPublic?: boolean }): Promise<number>;
  save(dashboard: Dashboard): Promise<void>;
  delete(organizationId: string, id: string): Promise<void>;
}
