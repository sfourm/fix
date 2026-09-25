import type { AuthenticateUserCommand } from '../commands/authenticate-user.command.js';
import type { RegisterUserCommand } from '../commands/register-user.command.js';
import { toSessionUser, toUserResponse } from '../mappers/user.mapper.js';
import type { AuthGateway } from '../ports/auth.gateway.js';
import type { SessionTokenPort } from '../ports/session-token.port.js';
import type { GetUserQuery } from '../queries/get-user.query.js';
import type { SessionResponse } from '../responses/session.response.js';
import type { UserResponse } from '../responses/user.response.js';

export class AuthService {
  constructor(
    private readonly gateway: AuthGateway,
    private readonly tokens: SessionTokenPort,
  ) {}

  /** Registra o usuário no Identity do core e já abre a sessão. */
  async register(command: RegisterUserCommand): Promise<SessionResponse> {
    await this.gateway.register(command);
    return this.authenticate({ email: command.email, password: command.password });
  }

  /** Valida as credenciais no core e emite o token de sessão do BFF. */
  async authenticate(command: AuthenticateUserCommand): Promise<SessionResponse> {
    const user = await this.gateway.authenticate(command);
    const { token, expiresAt } = await this.tokens.issue(toSessionUser(user));
    return { token, expiresAt, user: toUserResponse(user) };
  }

  async getUser(query: GetUserQuery): Promise<UserResponse> {
    return toUserResponse(await this.gateway.getUser(query));
  }
}
