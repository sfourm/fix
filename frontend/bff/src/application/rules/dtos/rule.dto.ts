export interface RuleDto {
  id: string;
  code: string;
  name: string;
  /** Códigos das roles concedidas. */
  roles: string[];
}
