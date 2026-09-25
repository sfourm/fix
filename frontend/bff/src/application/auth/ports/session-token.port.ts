import type { SessionUser } from '../../../cross-cutting/context/session-user.js';

/** Emissão e validação do token de sessão que o web envia no header Authorization. */
export interface SessionTokenPort {
  issue(user: SessionUser): Promise<{ token: string; expiresAt: string }>;
  verify(token: string): Promise<SessionUser>;
}
