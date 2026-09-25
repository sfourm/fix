import { randomUUID } from 'node:crypto';
import type { Actor } from '../common/actor.js';
import type { DataSource } from '../common/data-source.js';
import { ensure } from '../common/domain-error.js';
import { SharedResource, type SharedResourceProps } from '../common/shared-resource.js';
import type { Visibility } from '../common/visibility.js';
import { createCriteria, type FilterCriterion } from './filter-criterion.js';

export interface SavedFilterProps extends SharedResourceProps {
  source: DataSource;
  criteria: FilterCriterion[];
}

/** Filtro salvo: um conjunto de condições sobre um conjunto de dados, reutilizável em pesquisas e widgets. */
export class SavedFilter extends SharedResource<SavedFilterProps> {
  get source(): DataSource {
    return this.props.source;
  }

  get criteria(): readonly FilterCriterion[] {
    return this.props.criteria;
  }

  protected get label(): string {
    return 'O filtro';
  }

  static create(actor: Actor, input: { name: string; source: DataSource; criteria: FilterCriterion[]; visibility: Visibility }): SavedFilter {
    const now = new Date();
    return new SavedFilter({
      id: randomUUID(),
      organizationId: actor.organizationId,
      ownerId: actor.userId,
      name: SharedResource.validName(input.name, 'O filtro'),
      visibility: input.visibility,
      source: input.source,
      criteria: SavedFilter.validCriteria(input.criteria),
      createdAt: now,
      updatedAt: now,
    });
  }

  /** Reconstrói a partir do armazenamento (sem revalidar o que já foi validado ao salvar). */
  static restore(props: SavedFilterProps): SavedFilter {
    return new SavedFilter({ ...props, criteria: [...props.criteria] });
  }

  /** O conjunto de dados não muda: os campos das condições pertencem a ele. */
  changeCriteria(actor: Actor, criteria: FilterCriterion[]): void {
    this.ensureCanEdit(actor);
    this.props.criteria = SavedFilter.validCriteria(criteria);
    this.touch();
  }

  toSnapshot(): Readonly<SavedFilterProps> {
    return { ...this.props, criteria: [...this.props.criteria] };
  }

  private static validCriteria(criteria: FilterCriterion[]): FilterCriterion[] {
    ensure(criteria.length > 0, 'Um filtro salvo precisa de pelo menos uma condição.');
    return createCriteria(criteria);
  }
}
