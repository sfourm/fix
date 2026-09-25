import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RemoveWidgetCommand {
  context: RequestContext;
  dashboardId: string;
  widgetId: string;
}
