import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ReorderWidgetsCommand {
  context: RequestContext;
  dashboardId: string;
  widgetIds: string[];
}
