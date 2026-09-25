import type { Desk } from '../../../cross-cutting/enums/desk.js';

export interface MemberDto {
  id: string;
  userId: string;
  email: string;
  fullName: string;
  desk: Desk | null;
  rules: string[];
  groups: string[];
}
