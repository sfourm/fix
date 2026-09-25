/** Rule: conjunto de roles atribuído a membros e grupos (ex.: gestor, operador, middle_office). */
export interface Rule {
  id: string;
  code: string;
  name: string;
  /** Códigos das roles concedidas. */
  roles: string[];
}

/** Rules de plataforma/fundação não são atribuídas manualmente dentro de uma organização. */
export const isAssignableRule = (rule: Rule) => rule.code !== 'super_administrador' && rule.code !== 'founder';
