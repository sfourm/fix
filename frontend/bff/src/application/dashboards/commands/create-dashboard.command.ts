import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Visibility } from '../../../domain/common/visibility.js';
import type { DashboardTemplate } from '../templates/dashboard-templates.js';

export interface CreateDashboardCommand {
  context: RequestContext;
  name: string;
  description: string | null;
  visibility: Visibility;
  /** Modelo inicial de widgets ('blank' = vazio). */
  template: DashboardTemplate;
}
