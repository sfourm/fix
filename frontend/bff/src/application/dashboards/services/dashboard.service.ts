import { AppError } from '../../../cross-cutting/errors/app-error.js';
import type { Actor } from '../../../domain/common/actor.js';
import { DomainError } from '../../../domain/common/domain-error.js';
import type { Visibility } from '../../../domain/common/visibility.js';
import { Dashboard } from '../../../domain/dashboards/dashboard.js';
import type { DashboardRepository } from '../../../domain/dashboards/dashboard.repository.js';
import type { WidgetDefinition } from '../../../domain/dashboards/dashboard-widget.js';
import { createQuery } from '../../../domain/dashboards/widget-query.js';
import type { SavedFilterRepository } from '../../../domain/filters/saved-filter.repository.js';
import type { ActorResolver } from '../../common/actor-resolver.js';
import type { SearchService } from '../../search/services/search.service.js';
import type { WidgetDataResponse } from '../../search/responses/widget-data.response.js';
import type { AddWidgetCommand } from '../commands/add-widget.command.js';
import type { CreateDashboardCommand } from '../commands/create-dashboard.command.js';
import type { DeleteDashboardCommand } from '../commands/delete-dashboard.command.js';
import type { DuplicateDashboardCommand } from '../commands/duplicate-dashboard.command.js';
import type { RemoveWidgetCommand } from '../commands/remove-widget.command.js';
import type { ReorderWidgetsCommand } from '../commands/reorder-widgets.command.js';
import type { UpdateDashboardCommand } from '../commands/update-dashboard.command.js';
import type { UpdateWidgetCommand } from '../commands/update-widget.command.js';
import { toDashboardResponse, toDashboardSummaryResponse } from '../mappers/dashboard.mapper.js';
import type { GetDashboardQuery } from '../queries/get-dashboard.query.js';
import type { GetWidgetDataQuery } from '../queries/get-widget-data.query.js';
import type { ListDashboardsQuery } from '../queries/list-dashboards.query.js';
import type { DashboardSummaryResponse } from '../responses/dashboard-summary.response.js';
import type { DashboardResponse } from '../responses/dashboard.response.js';
import { dashboardTemplates } from '../templates/dashboard-templates.js';

/** Dashboards personalizáveis: CRUD, widgets (tipo, tamanho, dados, cores), visibilidade e dados dos widgets. */
export class DashboardService {
  constructor(
    private readonly dashboards: DashboardRepository,
    private readonly filters: SavedFilterRepository,
    private readonly search: SearchService,
    private readonly actors: ActorResolver,
  ) {}

  async list(query: ListDashboardsQuery): Promise<DashboardSummaryResponse[]> {
    const actor = await this.actors.resolve(query.context);
    const dashboards = await this.dashboards.listVisible(actor.organizationId, actor.userId);
    return dashboards.map((d) => toDashboardSummaryResponse(d, actor));
  }

  async get(query: GetDashboardQuery): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(query.context);
    return toDashboardResponse(await this.load(actor, query.id), actor);
  }

  async create(command: CreateDashboardCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const widgets = dashboardTemplates[command.template];
    const dashboard = Dashboard.create(actor, { ...command, widgets });

    await this.ensureWidgetsValid(actor, dashboard.visibility, widgets);
    await this.dashboards.save(dashboard);
    return toDashboardResponse(dashboard, actor);
  }

  async update(command: UpdateDashboardCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const dashboard = await this.load(actor, command.id);

    dashboard.rename(actor, command.name);
    dashboard.describe(actor, command.description);
    if (command.visibility !== dashboard.visibility) {
      if (command.visibility === 'Public') {
        await this.ensureFiltersPublic(actor, dashboard.widgets);
      }

      dashboard.changeVisibility(actor, command.visibility);
    }

    await this.dashboards.save(dashboard);
    return toDashboardResponse(dashboard, actor);
  }

  async delete(command: DeleteDashboardCommand): Promise<void> {
    const actor = await this.actors.resolve(command.context);
    const dashboard = await this.load(actor, command.id);
    dashboard.ensureCanEdit(actor);
    await this.dashboards.delete(actor.organizationId, dashboard.id);
  }

  async duplicate(command: DuplicateDashboardCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const copy = (await this.load(actor, command.id)).duplicate(actor, command.name ?? undefined);

    await this.dashboards.save(copy);
    return toDashboardResponse(copy, actor);
  }

  async addWidget(command: AddWidgetCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const dashboard = await this.load(actor, command.dashboardId);

    await this.ensureWidgetsValid(actor, dashboard.visibility, [command.widget]);
    dashboard.addWidget(actor, command.widget);
    await this.dashboards.save(dashboard);
    return toDashboardResponse(dashboard, actor);
  }

  async updateWidget(command: UpdateWidgetCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const dashboard = await this.load(actor, command.dashboardId);

    await this.ensureWidgetsValid(actor, dashboard.visibility, [command.widget]);
    dashboard.updateWidget(actor, command.widgetId, command.widget);
    await this.dashboards.save(dashboard);
    return toDashboardResponse(dashboard, actor);
  }

  async removeWidget(command: RemoveWidgetCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const dashboard = await this.load(actor, command.dashboardId);

    dashboard.removeWidget(actor, command.widgetId);
    await this.dashboards.save(dashboard);
    return toDashboardResponse(dashboard, actor);
  }

  async reorderWidgets(command: ReorderWidgetsCommand): Promise<DashboardResponse> {
    const actor = await this.actors.resolve(command.context);
    const dashboard = await this.load(actor, command.dashboardId);

    dashboard.reorderWidgets(actor, command.widgetIds);
    await this.dashboards.save(dashboard);
    return toDashboardResponse(dashboard, actor);
  }

  /** Dados do widget calculados com as roles de quem vê: sem view_order, widgets de boletas respondem 403. */
  async widgetData(query: GetWidgetDataQuery): Promise<WidgetDataResponse> {
    const actor = await this.actors.resolve(query.context);
    const widget = (await this.load(actor, query.dashboardId)).findWidget(query.widgetId);
    return this.search.aggregate(query.context, actor, widget.type, widget.query, { trustFilter: true });
  }

  private async load(actor: Actor, id: string): Promise<Dashboard> {
    const dashboard = await this.dashboards.findById(actor.organizationId, id);
    if (!dashboard || !dashboard.canView(actor)) {
      throw new AppError('not_found', 'Dashboard não encontrado.');
    }

    return dashboard;
  }

  /** Campos do conjunto de dados e filtro salvo: visível ao usuário, do mesmo conjunto e público se o dashboard for público. */
  private async ensureWidgetsValid(actor: Actor, visibility: Visibility, widgets: readonly WidgetDefinition[]): Promise<void> {
    for (const widget of widgets) {
      const query = createQuery(widget.type, widget.query);
      this.search.validateWidgetQuery(query);
      if (query.filterId) {
        await this.search.effectiveCriteria(actor, query.source, [], query.filterId);
      }
    }

    if (visibility === 'Public') {
      await this.ensureFiltersPublic(actor, widgets);
    }
  }

  private async ensureFiltersPublic(actor: Actor, widgets: readonly WidgetDefinition[]): Promise<void> {
    const ids = [...new Set(widgets.map((w) => w.query.filterId).filter((id): id is string => !!id))];
    for (const id of ids) {
      const filter = await this.filters.findById(actor.organizationId, id);
      if (filter && filter.visibility !== 'Public') {
        throw new DomainError(`O filtro "${filter.name}" é privado: dashboards públicos só usam filtros públicos.`);
      }
    }
  }
}
