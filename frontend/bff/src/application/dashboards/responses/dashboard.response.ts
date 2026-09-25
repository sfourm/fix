import type { DashboardSummaryResponse } from './dashboard-summary.response.js';
import type { WidgetResponse } from './widget.response.js';

export interface DashboardResponse extends DashboardSummaryResponse {
  widgets: WidgetResponse[];
}
