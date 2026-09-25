import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Visibility } from '../../../domain/common/visibility.js';

export interface UpdateDashboardCommand {
  context: RequestContext;
  id: string;
  name: string;
  description: string | null;
  visibility: Visibility;
}
