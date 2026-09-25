import type { Session, User } from '@/domain/session';
import type { HttpClient } from '../http/http-client';

export function createAuthApi(http: HttpClient) {
  return {
    login: (input: { email: string; password: string }) => http.post<Session>('/auth/login', input),
    register: (input: { email: string; password: string; fullName: string }) => http.post<Session>('/auth/register', input),
    me: () => http.get<User>('/auth/me'),
  };
}
