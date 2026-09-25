import type { Actor } from '../../../domain/common/actor.js';
import type { Dashboard } from '../../../domain/dashboards/dashboard.js';
import type { DashboardWidget } from '../../../domain/dashboards/dashboard-widget.js';
import type { DashboardSummaryResponse } from '../responses/dashboard-summary.response.js';
import type { DashboardResponse } from '../responses/dashboard.response.js';
import type { WidgetResponse } from '../responses/widget.response.js';

export const toWidgetResponse = (w: DashboardWidget): WidgetResponse => ({
  id: w.id,
  title: w.title,
  type: w.type,
  layout: { ...w.layout },
  query: { ...w.query, criteria: w.query.criteria.map((c) => ({ ...c })) },
  colors: { mode: w.colors.mode, palette: [...w.colors.palette] },
});

export const toDashboardSummaryResponse = (d: Dashboard, actor: Actor): DashboardSummaryResponse => ({
  id: d.id,
  name: d.name,
  description: d.description,
  visibility: d.visibility,
  ownerId: d.ownerId,
  isMine: d.isOwnedBy(actor.userId),
  canEdit: d.canEdit(actor),
  widgetCount: d.widgets.length,
  updatedAt: d.updatedAt.toISOString(),
});

export const toDashboardResponse = (d: Dashboard, actor: Actor): DashboardResponse => ({
  ...toDashboardSummaryResponse(d, actor),
  widgets: d.widgets.map(toWidgetResponse),
});
