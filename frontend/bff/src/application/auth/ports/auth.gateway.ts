import type { AuthenticateUserCommand } from '../commands/authenticate-user.command.js';
import type { RegisterUserCommand } from '../commands/register-user.command.js';
import type { UserDto } from '../dtos/user.dto.js';
import type { GetUserQuery } from '../queries/get-user.query.js';

/** Porta para o Identity do core (implementada via gRPC na infraestrutura). */
export interface AuthGateway {
  register(command: RegisterUserCommand): Promise<string>;
  authenticate(command: AuthenticateUserCommand): Promise<UserDto>;
  getUser(query: GetUserQuery): Promise<UserDto>;
}
