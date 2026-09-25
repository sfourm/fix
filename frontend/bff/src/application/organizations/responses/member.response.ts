import type { Desk } from '../../../cross-cutting/enums/desk.js';

export interface MemberResponse {
  id: string;
  userId: string;
  email: string;
  fullName: string;
  desk: Desk | null;
  /** Base do membro: owner, user (FIX: super_administrador, administrador). */
  role: string;
  isOwner: boolean;
  /** Alçadas atribuídas diretamente ao membro. */
  rules: string[];
  groups: string[];
}
