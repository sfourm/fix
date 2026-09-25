import { AppError } from '../../../cross-cutting/errors/app-error.js';
import type { Actor } from '../../../domain/common/actor.js';
import { DomainError } from '../../../domain/common/domain-error.js';
import type { DashboardRepository } from '../../../domain/dashboards/dashboard.repository.js';
import { SavedFilter } from '../../../domain/filters/saved-filter.js';
import type { SavedFilterRepository } from '../../../domain/filters/saved-filter.repository.js';
import type { ActorResolver } from '../../common/actor-resolver.js';
import { validateCriteria } from '../../search/engine/criteria-engine.js';
import type { CreateSavedFilterCommand } from '../commands/create-saved-filter.command.js';
import type { DeleteSavedFilterCommand } from '../commands/delete-saved-filter.command.js';
import type { UpdateSavedFilterCommand } from '../commands/update-saved-filter.command.js';
import { toSavedFilterResponse } from '../mappers/saved-filter.mapper.js';
import type { GetSavedFilterQuery } from '../queries/get-saved-filter.query.js';
import type { ListSavedFiltersQuery } from '../queries/list-saved-filters.query.js';
import type { SavedFilterResponse } from '../responses/saved-filter.response.js';

/** Filtros salvos: condições reutilizáveis nas pesquisas e nos widgets, privados ou públicos na organização. */
export class SavedFilterService {
  constructor(
    private readonly filters: SavedFilterRepository,
    private readonly dashboards: DashboardRepository,
    private readonly actors: ActorResolver,
  ) {}

  async create(command: CreateSavedFilterCommand): Promise<SavedFilterResponse> {
    const actor = await this.actors.resolve(command.context);
    const filter = SavedFilter.create(actor, command);
    validateCriteria(filter.source, filter.criteria);

    await this.filters.save(filter);
    return toSavedFilterResponse(filter, actor);
  }

  async update(command: UpdateSavedFilterCommand): Promise<SavedFilterResponse> {
    const actor = await this.actors.resolve(command.context);
    const filter = await this.load(actor, command.id);

    filter.rename(actor, command.name);
    filter.changeCriteria(actor, command.criteria);
    validateCriteria(filter.source, filter.criteria);

    if (command.visibility !== filter.visibility) {
      // Dashboards públicos só usam filtros públicos: quem vê o dashboard precisa conseguir aplicar o filtro.
      if (command.visibility === 'Private' && (await this.dashboards.countUsingFilter(actor.organizationId, filter.id, { onlyPublic: true })) > 0) {
        throw new DomainError('O filtro é usado por dashboards públicos e não pode ficar privado.');
      }

      filter.changeVisibility(actor, command.visibility);
    }

    await this.filters.save(filter);
    return toSavedFilterResponse(filter, actor);
  }

  async delete(command: DeleteSavedFilterCommand): Promise<void> {
    const actor = await this.actors.resolve(command.context);
    const filter = await this.load(actor, command.id);
    filter.ensureCanEdit(actor);

    const usage = await this.dashboards.countUsingFilter(actor.organizationId, filter.id);
    if (usage > 0) {
      throw new DomainError(`O filtro é usado por ${usage} dashboard(s): troque o filtro dos widgets antes de excluí-lo.`);
    }

    await this.filters.delete(actor.organizationId, filter.id);
  }

  async get(query: GetSavedFilterQuery): Promise<SavedFilterResponse> {
    const actor = await this.actors.resolve(query.context);
    const filter = await this.load(actor, query.id);
    filter.ensureCanView(actor);
    return toSavedFilterResponse(filter, actor);
  }

  async list(query: ListSavedFiltersQuery): Promise<SavedFilterResponse[]> {
    const actor = await this.actors.resolve(query.context);
    const filters = await this.filters.listVisible(actor.organizationId, actor.userId, query.source ?? undefined);
    return filters.map((f) => toSavedFilterResponse(f, actor));
  }

  private async load(actor: Actor, id: string): Promise<SavedFilter> {
    const filter = await this.filters.findById(actor.organizationId, id);
    if (!filter || !filter.canView(actor)) {
      throw new AppError('not_found', 'Filtro salvo não encontrado.');
    }

    return filter;
  }
}
