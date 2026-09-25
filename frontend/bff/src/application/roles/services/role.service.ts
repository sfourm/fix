import { toRoleResponse } from '../mappers/role.mapper.js';
import type { RoleGateway } from '../ports/role.gateway.js';
import type { ListRolesQuery } from '../queries/list-roles.query.js';
import type { RoleResponse } from '../responses/role.response.js';

/** Roles: permissões atômicas do sistema. */
export class RoleService {
  constructor(private readonly gateway: RoleGateway) {}

  async list(query: ListRolesQuery): Promise<RoleResponse[]> {
    return (await this.gateway.list(query)).map(toRoleResponse);
  }
}
