/** Motivo da violação: regra de negócio (422) ou ação não permitida ao usuário (403). */
export type DomainErrorKind = 'rule' | 'forbidden';

/** Erro lançado pelas entidades do domínio do BFF quando uma regra é violada. */
export class DomainError extends Error {
  constructor(
    message: string,
    readonly kind: DomainErrorKind = 'rule',
  ) {
    super(message);
    this.name = 'DomainError';
  }

  static forbidden(message: string): DomainError {
    return new DomainError(message, 'forbidden');
  }
}

export function ensure(condition: boolean, message: string): asserts condition {
  if (!condition) {
    throw new DomainError(message);
  }
}
