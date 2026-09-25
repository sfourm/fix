import { randomUUID } from 'node:crypto';
import { ensure } from '../common/domain-error.js';
import { createColors, type WidgetColors } from './widget-colors.js';
import { createLayout, type WidgetLayout } from './widget-layout.js';
import { createQuery, type WidgetQuery } from './widget-query.js';
import type { WidgetType } from './widget-type.js';

export interface WidgetDefinition {
  title: string;
  type: WidgetType;
  layout: WidgetLayout;
  query: WidgetQuery;
  colors: WidgetColors;
}

/** Widget do dashboard (entidade filha): o id é estável enquanto título, tipo, tamanho, dados e cores mudam. */
export interface DashboardWidget extends WidgetDefinition {
  readonly id: string;
}

export function createWidget(definition: WidgetDefinition, id: string = randomUUID()): DashboardWidget {
  const title = definition.title.trim();
  ensure(title.length > 0 && title.length <= 80, 'O título do widget deve ter entre 1 e 80 caracteres.');

  return Object.freeze({
    id,
    title,
    type: definition.type,
    layout: createLayout(definition.layout.width, definition.layout.height),
    query: createQuery(definition.type, definition.query),
    colors: createColors(definition.colors.mode, definition.colors.palette),
  });
}
