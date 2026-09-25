import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { WidgetDefinition } from '../../../domain/dashboards/dashboard-widget.js';

/** Substitui a definição do widget: título, tipo, largura/altura, dados e cores. */
export interface UpdateWidgetCommand {
  context: RequestContext;
  dashboardId: string;
  widgetId: string;
  widget: WidgetDefinition;
}
