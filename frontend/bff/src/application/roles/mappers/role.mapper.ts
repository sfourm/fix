import type { RoleDto } from '../dtos/role.dto.js';
import type { RoleResponse } from '../responses/role.response.js';

export const toRoleResponse = (dto: RoleDto): RoleResponse => ({ ...dto });
