import type { Compliance, Mandate, MandateIssueInput, MandateStatus, MandateTerms } from '@/domain/mandate';
import type { Page } from '@/domain/page';
import type { HttpClient } from '../http/http-client';
import type { PageQuery } from './page-query';

export function createMandateApi(http: HttpClient) {
  return {
    list: (query: PageQuery & { policyId?: string; status?: MandateStatus }) => http.get<Page<Mandate>>('/mandates', { ...query }),
    get: (id: string) => http.get<Mandate>(`/mandates/${id}`),
    issue: (input: MandateIssueInput) => http.post<Mandate>('/mandates', input),
    /** Checagem de aderência à política antes de emitir (não grava nada). */
    preview: (input: MandateIssueInput) => http.post<Compliance>('/mandates/preview', input),
    update: (id: string, terms: MandateTerms) => http.put<Mandate>(`/mandates/${id}`, { terms }),
    remove: (id: string) => http.delete(`/mandates/${id}`),
    approve: (id: string, note: string | null) => http.post<Mandate>(`/mandates/${id}/approve`, { note }),
    reject: (id: string, note: string) => http.post<Mandate>(`/mandates/${id}/reject`, { note }),
    close: (id: string, note: string | null) => http.post<Mandate>(`/mandates/${id}/close`, { note }),
  };
}
