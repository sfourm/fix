export type ApiErrorCode =
  | 'validation'
  | 'unauthenticated'
  | 'forbidden'
  | 'not_found'
  | 'conflict'
  | 'business_rule'
  | 'unavailable'
  | 'internal'
  | 'network';

/** Erro devolvido pelo BFF ({ code, message, fields }). */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: ApiErrorCode,
    message: string,
    readonly fields: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'ApiError';
  }

  /** Primeira mensagem de um campo (as chaves podem vir do BFF em camelCase ou do core em PascalCase). */
  field(name: string): string | undefined {
    const key = Object.keys(this.fields).find((k) => k.toLowerCase() === name.toLowerCase());
    return key ? this.fields[key]?.[0] : undefined;
  }
}

export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message;
  }

  return error instanceof Error ? error.message : 'Erro inesperado.';
}
