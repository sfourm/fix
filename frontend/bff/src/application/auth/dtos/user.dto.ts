/** Usuário como vem do core (Identity). */
export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}
