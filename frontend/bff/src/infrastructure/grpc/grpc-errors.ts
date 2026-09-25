import { status, type ServiceError } from '@grpc/grpc-js';
import { AppError, type ErrorCode, type FieldErrors } from '../../cross-cutting/errors/app-error.js';

const VALIDATION_TRAILER = 'validation-errors';

const codes: Partial<Record<status, ErrorCode>> = {
  [status.INVALID_ARGUMENT]: 'validation',
  [status.UNAUTHENTICATED]: 'unauthenticated',
  [status.PERMISSION_DENIED]: 'forbidden',
  [status.NOT_FOUND]: 'not_found',
  [status.ALREADY_EXISTS]: 'conflict',
  [status.FAILED_PRECONDITION]: 'business_rule',
  [status.UNAVAILABLE]: 'unavailable',
  [status.DEADLINE_EXCEEDED]: 'unavailable',
};

/** Traduz o status gRPC do core para o erro de aplicação do BFF. */
export function toAppError(error: ServiceError): AppError {
  const code = codes[error.code] ?? 'internal';

  if (code === 'unavailable') {
    return new AppError(code, 'O serviço core está indisponível. Tente novamente em instantes.');
  }

  if (code === 'internal') {
    return new AppError(code, 'Erro inesperado ao processar a requisição.');
  }

  return new AppError(code, error.details || error.message, readFieldErrors(error));
}

function readFieldErrors(error: ServiceError): FieldErrors | undefined {
  const raw = error.metadata?.get(VALIDATION_TRAILER)[0];
  if (typeof raw !== 'string') {
    return undefined;
  }

  try {
    return JSON.parse(raw) as FieldErrors;
  } catch {
    return undefined;
  }
}
