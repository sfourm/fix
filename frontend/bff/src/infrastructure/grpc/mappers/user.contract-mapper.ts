import type { UserDto } from '../../../application/auth/dtos/user.dto.js';

export interface ContractUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export const toUserDto = (u: ContractUser): UserDto => ({ id: u.id, email: u.email, fullName: u.fullName, roles: u.roles });
