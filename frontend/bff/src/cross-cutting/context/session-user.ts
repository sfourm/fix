/** Usuário autenticado na sessão do BFF (conteúdo do token de sessão). */
export interface SessionUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}
