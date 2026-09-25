import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Dados de um widget salvo, calculados com o contexto (e as roles) de quem está vendo. */
export interface GetWidgetDataQuery {
  context: RequestContext;
  dashboardId: string;
  widgetId: string;
}
