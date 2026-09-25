import type { Dashboard, DashboardSummary, WidgetData, WidgetDefinition } from '@/domain/dashboard';
import type { Visibility } from '@/domain/search';
import type { HttpClient } from '../http/http-client';

export type DashboardTemplate = 'blank' | 'fix-overview';

export function createDashboardApi(http: HttpClient) {
  const base = (id: string) => `/dashboards/${id}`;

  return {
    list: () => http.get<DashboardSummary[]>('/dashboards'),
    get: (id: string) => http.get<Dashboard>(base(id)),
    create: (input: { name: string; description: string | null; visibility: Visibility; template: DashboardTemplate }) =>
      http.post<Dashboard>('/dashboards', input),
    update: (id: string, input: { name: string; description: string | null; visibility: Visibility }) => http.put<Dashboard>(base(id), input),
    remove: (id: string) => http.delete(base(id)),
    duplicate: (id: string, name: string | null = null) => http.post<Dashboard>(`${base(id)}/duplicate`, { name }),

    addWidget: (id: string, widget: WidgetDefinition) => http.post<Dashboard>(`${base(id)}/widgets`, widget),
    updateWidget: (id: string, widgetId: string, widget: WidgetDefinition) => http.put<Dashboard>(`${base(id)}/widgets/${widgetId}`, widget),
    removeWidget: (id: string, widgetId: string) => http.delete<Dashboard>(`${base(id)}/widgets/${widgetId}`),
    reorderWidgets: (id: string, widgetIds: string[]) => http.put<Dashboard>(`${base(id)}/widgets/order`, { widgetIds }),
    widgetData: (id: string, widgetId: string) => http.get<WidgetData>(`${base(id)}/widgets/${widgetId}/data`),
  };
}
