import type { SessionUser } from '../../../cross-cutting/context/session-user.js';
import type { UserDto } from '../dtos/user.dto.js';
import type { UserResponse } from '../responses/user.response.js';

export const toUserResponse = (dto: UserDto): UserResponse => ({
  id: dto.id,
  email: dto.email,
  fullName: dto.fullName,
  roles: dto.roles,
});

export const toSessionUser = (dto: UserDto): SessionUser => ({
  id: dto.id,
  email: dto.email,
  fullName: dto.fullName,
  roles: dto.roles,
});
