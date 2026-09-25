import type { Role } from '@/domain/role';
import type { HttpClient } from '../http/http-client';

export function createRoleApi(http: HttpClient) {
  return {
    list: () => http.get<Role[]>('/roles'),
  };
}
