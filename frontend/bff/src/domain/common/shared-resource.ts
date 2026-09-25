import type { Actor } from './actor.js';
import { DomainError, ensure } from './domain-error.js';
import type { Visibility } from './visibility.js';

export interface SharedResourceProps {
  id: string;
  organizationId: string;
  ownerId: string;
  name: string;
  visibility: Visibility;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * Base dos recursos salvos por usuário (dashboards e filtros): pertencem a uma organização e a um dono,
 * e podem ser privados (só o dono) ou públicos (toda a organização vê; editam o dono e quem gerencia a organização).
 */
export abstract class SharedResource<TProps extends SharedResourceProps> {
  protected constructor(protected readonly props: TProps) {}

  get id(): string {
    return this.props.id;
  }

  get organizationId(): string {
    return this.props.organizationId;
  }

  get ownerId(): string {
    return this.props.ownerId;
  }

  get name(): string {
    return this.props.name;
  }

  get visibility(): Visibility {
    return this.props.visibility;
  }

  get createdAt(): Date {
    return this.props.createdAt;
  }

  get updatedAt(): Date {
    return this.props.updatedAt;
  }

  isOwnedBy(userId: string): boolean {
    return this.props.ownerId === userId;
  }

  canView(actor: Actor): boolean {
    return actor.organizationId === this.props.organizationId && (this.props.visibility === 'Public' || this.isOwnedBy(actor.userId));
  }

  canEdit(actor: Actor): boolean {
    return this.canView(actor) && (this.isOwnedBy(actor.userId) || (this.props.visibility === 'Public' && actor.canManageShared));
  }

  ensureCanView(actor: Actor): void {
    if (!this.canView(actor)) {
      throw DomainError.forbidden(`${this.label} não está disponível para você.`);
    }
  }

  ensureCanEdit(actor: Actor): void {
    this.ensureCanView(actor);
    if (!this.canEdit(actor)) {
      throw DomainError.forbidden(`Só o dono pode alterar ${this.label.toLowerCase()} (ou quem gerencia a organização, se for público).`);
    }
  }

  rename(actor: Actor, name: string): void {
    this.ensureCanEdit(actor);
    this.props.name = SharedResource.validName(name, this.label);
    this.touch();
  }

  /** Tornar privado um recurso de outro usuário o esconderia do próprio dono do ato: só o dono muda a visibilidade. */
  changeVisibility(actor: Actor, visibility: Visibility): void {
    this.ensureCanEdit(actor);
    if (!this.isOwnedBy(actor.userId)) {
      throw DomainError.forbidden(`Só o dono pode mudar a visibilidade de ${this.label.toLowerCase()}.`);
    }

    this.props.visibility = visibility;
    this.touch();
  }

  protected touch(): void {
    this.props.updatedAt = new Date();
  }

  /** Nome do tipo de recurso nas mensagens (ex.: "O dashboard"). */
  protected abstract get label(): string;

  protected static validName(name: string, label: string): string {
    const trimmed = name.trim();
    ensure(trimmed.length > 0 && trimmed.length <= 120, `${label}: o nome deve ter entre 1 e 120 caracteres.`);
    return trimmed;
  }
}
