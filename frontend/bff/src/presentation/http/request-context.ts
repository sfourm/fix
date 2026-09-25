import type { Request } from 'express';
import type { RequestContext } from '../../cross-cutting/context/request-context.js';
import type { SessionUser } from '../../cross-cutting/context/session-user.js';
import { AppError } from '../../cross-cutting/errors/app-error.js';

declare module 'express-serve-static-core' {
  interface Request {
    sessionUser?: SessionUser;
    organizationId?: string;
  }
}

/** Contexto para operações do usuário (sem tenant). */
export function userContext(req: Request): RequestContext {
  if (!req.sessionUser) {
    throw AppError.unauthenticated();
  }

  return { userId: req.sessionUser.id };
}

/** Contexto para operações dentro da organização informada no header X-Organization-Id. */
export function organizationContext(req: Request): RequestContext {
  const ctx = userContext(req);
  if (!req.organizationId) {
    throw AppError.validation('Informe a organização no header X-Organization-Id.');
  }

  return { ...ctx, organizationId: req.organizationId };
}
