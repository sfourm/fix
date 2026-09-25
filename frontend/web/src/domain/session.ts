export interface User {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface Session {
  token: string;
  expiresAt: string;
  user: User;
}
