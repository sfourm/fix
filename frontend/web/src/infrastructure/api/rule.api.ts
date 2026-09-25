import type { Rule } from '@/domain/rule';
import type { HttpClient } from '../http/http-client';

export function createRuleApi(http: HttpClient) {
  return {
    list: () => http.get<Rule[]>('/rules'),
  };
}
