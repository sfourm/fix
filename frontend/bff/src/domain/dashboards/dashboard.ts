import { randomUUID } from 'node:crypto';
import type { Actor } from '../common/actor.js';
import { DomainError, ensure } from '../common/domain-error.js';
import { SharedResource, type SharedResourceProps } from '../common/shared-resource.js';
import type { Visibility } from '../common/visibility.js';
import { createWidget, type DashboardWidget, type WidgetDefinition } from './dashboard-widget.js';

export const MAX_WIDGETS = 24;

export interface DashboardProps extends SharedResourceProps {
  description: string | null;
  widgets: DashboardWidget[];
}

/** Dashboard personalizável: widgets em grade, na ordem em que aparecem. Privado do dono ou público na organização. */
export class Dashboard extends SharedResource<DashboardProps> {
  get description(): string | null {
    return this.props.description;
  }

  get widgets(): readonly DashboardWidget[] {
    return this.props.widgets;
  }

  protected get label(): string {
    return 'O dashboard';
  }

  static create(
    actor: Actor,
    input: { name: string; description: string | null; visibility: Visibility; widgets?: WidgetDefinition[] },
  ): Dashboard {
    const widgets = (input.widgets ?? []).map((w) => createWidget(w));
    ensure(widgets.length <= MAX_WIDGETS, `No máximo ${MAX_WIDGETS} widgets por dashboard.`);

    const now = new Date();
    return new Dashboard({
      id: randomUUID(),
      organizationId: actor.organizationId,
      ownerId: actor.userId,
      name: SharedResource.validName(input.name, 'O dashboard'),
      description: Dashboard.validDescription(input.description),
      visibility: input.visibility,
      widgets,
      createdAt: now,
      updatedAt: now,
    });
  }

  static restore(props: DashboardProps): Dashboard {
    return new Dashboard({ ...props, widgets: [...props.widgets] });
  }

  /** Cópia privada para o usuário personalizar (ex.: a partir de um dashboard público). */
  duplicate(actor: Actor, name?: string): Dashboard {
    this.ensureCanView(actor);
    return Dashboard.create(actor, {
      name: name ?? `${this.name} (cópia)`.slice(0, 120),
      description: this.description,
      visibility: 'Private',
      widgets: this.widgets.map(({ id: _, ...definition }) => definition),
    });
  }

  describe(actor: Actor, description: string | null): void {
    this.ensureCanEdit(actor);
    this.props.description = Dashboard.validDescription(description);
    this.touch();
  }

  addWidget(actor: Actor, definition: WidgetDefinition): DashboardWidget {
    this.ensureCanEdit(actor);
    ensure(this.props.widgets.length < MAX_WIDGETS, `No máximo ${MAX_WIDGETS} widgets por dashboard.`);

    const widget = createWidget(definition);
    this.props.widgets = [...this.props.widgets, widget];
    this.touch();
    return widget;
  }

  updateWidget(actor: Actor, widgetId: string, definition: WidgetDefinition): DashboardWidget {
    this.ensureCanEdit(actor);
    const index = this.indexOf(widgetId);

    const widget = createWidget(definition, widgetId);
    this.props.widgets = this.props.widgets.map((w, i) => (i === index ? widget : w));
    this.touch();
    return widget;
  }

  removeWidget(actor: Actor, widgetId: string): void {
    this.ensureCanEdit(actor);
    const index = this.indexOf(widgetId);
    this.props.widgets = this.props.widgets.filter((_, i) => i !== index);
    this.touch();
  }

  /** Reordena a grade: a lista precisa conter exatamente os widgets atuais. */
  reorderWidgets(actor: Actor, widgetIds: string[]): void {
    this.ensureCanEdit(actor);
    const current = new Map(this.props.widgets.map((w) => [w.id, w]));
    ensure(
      widgetIds.length === current.size && new Set(widgetIds).size === current.size && widgetIds.every((id) => current.has(id)),
      'A nova ordem precisa conter todos os widgets do dashboard, uma única vez.',
    );

    this.props.widgets = widgetIds.map((id) => current.get(id)!);
    this.touch();
  }

  findWidget(widgetId: string): DashboardWidget {
    return this.props.widgets[this.indexOf(widgetId)]!;
  }

  toSnapshot(): Readonly<DashboardProps> {
    return { ...this.props, widgets: [...this.props.widgets] };
  }

  private indexOf(widgetId: string): number {
    const index = this.props.widgets.findIndex((w) => w.id === widgetId);
    if (index < 0) {
      throw new DomainError('Widget não encontrado neste dashboard.');
    }

    return index;
  }

  private static validDescription(description: string | null): string | null {
    const trimmed = description?.trim() ?? '';
    ensure(trimmed.length <= 500, 'A descrição do dashboard deve ter no máximo 500 caracteres.');
    return trimmed || null;
  }
}
