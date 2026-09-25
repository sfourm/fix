import type { RoleDto } from '../dtos/role.dto.js';
import type { ListRolesQuery } from '../queries/list-roles.query.js';

export interface RoleGateway {
  list(query: ListRolesQuery): Promise<RoleDto[]>;
}
