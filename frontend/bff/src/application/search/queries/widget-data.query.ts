import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { WidgetQuery } from '../../../domain/dashboards/widget-query.js';
import type { WidgetType } from '../../../domain/dashboards/widget-type.js';

/** Prévia de um widget ainda não salvo (editor): consulta montada pelo usuário. */
export interface WidgetDataQuery {
  context: RequestContext;
  type: WidgetType;
  query: WidgetQuery;
}
