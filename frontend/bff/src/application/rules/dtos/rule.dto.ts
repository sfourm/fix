export interface RuleDto {
  id: string;
  code: string;
  name: string;
  /** Códigos das roles concedidas. */
  roles: string[];
  /** owner, user e as internas da FIX: não editáveis. */
  isSystem: boolean;
  /** Membros e grupos que usam a rule na organização. */
  usages: number;
}
