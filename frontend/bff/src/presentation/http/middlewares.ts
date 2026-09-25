import type { ErrorRequestHandler, RequestHandler } from 'express';
import { z } from 'zod';
import type { SessionTokenPort } from '../../application/auth/ports/session-token.port.js';
import { AppError, type ErrorCode } from '../../cross-cutting/errors/app-error.js';

export const ORGANIZATION_HEADER = 'x-organization-id';

/** Exige o header Authorization: Bearer <token de sessão> em toda chamada autenticada do web. */
export function authenticate(tokens: SessionTokenPort): RequestHandler {
  return async (req, _res, next) => {
    const header = req.header('authorization') ?? '';
    const [scheme, token] = header.split(' ');
    if (scheme?.toLowerCase() !== 'bearer' || !token) {
      throw AppError.unauthenticated('Header Authorization ausente.');
    }

    req.sessionUser = await tokens.verify(token);
    next();
  };
}

/** Lê o tenant do header X-Organization-Id (a autorização por role acontece no core, consultando a base). */
export const resolveOrganization: RequestHandler = (req, _res, next) => {
  const value = req.header(ORGANIZATION_HEADER);
  if (value) {
    if (!z.uuid().safeParse(value).success) {
      throw AppError.validation('O header X-Organization-Id deve ser um UUID válido.');
    }

    req.organizationId = value;
  }

  next();
};

const httpStatus: Record<ErrorCode, number> = {
  validation: 400,
  unauthenticated: 401,
  forbidden: 403,
  not_found: 404,
  conflict: 409,
  business_rule: 422,
  unavailable: 503,
  internal: 500,
};

export const notFound: RequestHandler = (req) => {
  throw new AppError('not_found', `Rota ${req.method} ${req.path} não encontrada.`);
};

export const errorHandler: ErrorRequestHandler = (error, _req, res, _next) => {
  if (error instanceof AppError) {
    res.status(httpStatus[error.code]).json({ code: error.code, message: error.message, fields: error.fields });
    return;
  }

  if (error instanceof SyntaxError && 'body' in error) {
    res.status(400).json({ code: 'validation', message: 'JSON inválido.' });
    return;
  }

  console.error(error);
  res.status(500).json({ code: 'internal', message: 'Erro inesperado.' });
};
