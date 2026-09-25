import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { WidgetDefinition } from '../../../domain/dashboards/dashboard-widget.js';

export interface AddWidgetCommand {
  context: RequestContext;
  dashboardId: string;
  widget: WidgetDefinition;
}
