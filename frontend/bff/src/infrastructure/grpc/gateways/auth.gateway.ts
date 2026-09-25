import type { AuthenticateUserCommand } from '../../../application/auth/commands/authenticate-user.command.js';
import type { RegisterUserCommand } from '../../../application/auth/commands/register-user.command.js';
import type { UserDto } from '../../../application/auth/dtos/user.dto.js';
import type { AuthGateway } from '../../../application/auth/ports/auth.gateway.js';
import type { GetUserQuery } from '../../../application/auth/queries/get-user.query.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext } from '../mappers/common.contract-mapper.js';
import { toUserDto, type ContractUser } from '../mappers/user.contract-mapper.js';

const SERVICE = 'AuthService';

export class GrpcAuthGateway implements AuthGateway {
  constructor(private readonly core: CoreClient) {}

  async register(command: RegisterUserCommand): Promise<string> {
    const response = await this.core.call<{ userId: string }>(SERVICE, 'Register', {
      email: command.email,
      password: command.password,
      fullName: command.fullName,
    });
    return response.userId;
  }

  async authenticate(command: AuthenticateUserCommand): Promise<UserDto> {
    return toUserDto(
      await this.core.call<ContractUser>(SERVICE, 'Authenticate', { email: command.email, password: command.password }),
    );
  }

  async getUser(query: GetUserQuery): Promise<UserDto> {
    return toUserDto(await this.core.call<ContractUser>(SERVICE, 'GetUser', { context: toContractContext(query.context) }));
  }
}
