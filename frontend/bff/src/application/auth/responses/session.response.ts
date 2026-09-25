import type { UserResponse } from './user.response.js';

/** Sessão emitida pelo BFF: o web envia o token no header Authorization. */
export interface SessionResponse {
  token: string;
  expiresAt: string;
  user: UserResponse;
}
