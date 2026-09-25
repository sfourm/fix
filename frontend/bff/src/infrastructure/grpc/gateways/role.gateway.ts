import type { RoleDto } from '../../../application/roles/dtos/role.dto.js';
import type { RoleGateway } from '../../../application/roles/ports/role.gateway.js';
import type { CoreClient } from '../core-client.js';

export class GrpcRoleGateway implements RoleGateway {
  constructor(private readonly core: CoreClient) {}

  async list(): Promise<RoleDto[]> {
    const response = await this.core.call<{ roles: RoleDto[] }>('RoleService', 'ListRoles', {});
    return response.roles.map((r) => ({ id: r.id, code: r.code, description: r.description }));
  }
}
