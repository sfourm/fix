export type ErrorCode =
  | 'validation'
  | 'unauthenticated'
  | 'forbidden'
  | 'not_found'
  | 'conflict'
  | 'business_rule'
  | 'unavailable'
  | 'internal';

export type FieldErrors = Record<string, string[]>;

/** Erro padrão do BFF; a presentation converte o code em status HTTP. */
export class AppError extends Error {
  constructor(
    readonly code: ErrorCode,
    message: string,
    readonly fields?: FieldErrors,
  ) {
    super(message);
    this.name = 'AppError';
  }

  static validation(message: string, fields?: FieldErrors): AppError {
    return new AppError('validation', message, fields);
  }

  static unauthenticated(message = 'Sessão inválida ou expirada.'): AppError {
    return new AppError('unauthenticated', message);
  }
}
