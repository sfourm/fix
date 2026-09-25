import type { Rule } from '@/domain/rule';
import type { HttpClient } from '../http/http-client';

export interface RuleInput {
  name: string;
  roleCodes: string[];
}

/** Rules da organização atual (header X-Organization-Id): owner, user e as alçadas personalizadas. */
export function createRuleApi(http: HttpClient) {
  return {
    list: () => http.get<Rule[]>('/rules'),
    create: (input: RuleInput) => http.post<Rule>('/rules', input),
    update: (id: string, input: RuleInput) => http.put<Rule>(`/rules/${id}`, input),
    remove: (id: string) => http.delete(`/rules/${id}`),
  };
}
