import { errors, jwtVerify, SignJWT } from 'jose';
import type { SessionTokenPort } from '../../application/auth/ports/session-token.port.js';
import type { SessionUser } from '../../cross-cutting/context/session-user.js';
import { AppError } from '../../cross-cutting/errors/app-error.js';

const ISSUER = 'fix-bff';
const AUDIENCE = 'fix-web';

/** Token de sessão (JWT HS256) emitido pelo BFF para o web. O core nunca vê este token. */
export class JwtSessionTokenService implements SessionTokenPort {
  private readonly key: Uint8Array;

  constructor(
    secret: string,
    private readonly ttl: string,
  ) {
    this.key = new TextEncoder().encode(secret);
  }

  async issue(user: SessionUser): Promise<{ token: string; expiresAt: string }> {
    const token = await new SignJWT({ email: user.email, name: user.fullName, roles: user.roles })
      .setProtectedHeader({ alg: 'HS256' })
      .setSubject(user.id)
      .setIssuer(ISSUER)
      .setAudience(AUDIENCE)
      .setIssuedAt()
      .setExpirationTime(this.ttl)
      .sign(this.key);

    const { payload } = await jwtVerify(token, this.key);
    return { token, expiresAt: new Date((payload.exp ?? 0) * 1000).toISOString() };
  }

  async verify(token: string): Promise<SessionUser> {
    try {
      const { payload } = await jwtVerify(token, this.key, { issuer: ISSUER, audience: AUDIENCE });
      if (!payload.sub) {
        throw AppError.unauthenticated();
      }

      return {
        id: payload.sub,
        email: String(payload['email'] ?? ''),
        fullName: String(payload['name'] ?? ''),
        roles: Array.isArray(payload['roles']) ? (payload['roles'] as string[]) : [],
      };
    } catch (error) {
      if (error instanceof errors.JWTExpired) {
        throw AppError.unauthenticated('Sessão expirada. Entre novamente.');
      }

      throw error instanceof AppError ? error : AppError.unauthenticated();
    }
  }
}
