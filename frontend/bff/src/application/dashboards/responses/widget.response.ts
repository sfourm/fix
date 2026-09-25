import type { WidgetDefinition } from '../../../domain/dashboards/dashboard-widget.js';

export interface WidgetResponse extends WidgetDefinition {
  id: string;
}
