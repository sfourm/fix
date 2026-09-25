export interface RuleResponse {
  id: string;
  code: string;
  name: string;
  /** Códigos das roles concedidas. */
  roles: string[];
}
