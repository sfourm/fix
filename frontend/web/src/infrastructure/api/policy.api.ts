import type { Page } from '@/domain/page';
import type {
  CoverageBandInput,
  Policy,
  PolicyAxisInput,
  PolicyInstrumentInput,
  PolicyLimits,
  PolicySummary,
} from '@/domain/policy';
import type { HttpClient } from '../http/http-client';
import type { PageQuery } from './page-query';

export interface PolicyHeaderInput {
  code: string;
  title: string;
  description: string | null;
  validFrom: string;
  validTo: string | null;
}

export function createPolicyApi(http: HttpClient) {
  const base = (id: string) => `/policies/${id}`;

  return {
    list: (query: PageQuery) => http.get<Page<PolicySummary>>('/policies', { ...query }),
    get: (id: string) => http.get<Policy>(base(id)),
    create: (input: PolicyHeaderInput & { version: string; useTemplate: boolean }) => http.post<Policy>('/policies', input),
    update: (id: string, input: PolicyHeaderInput) => http.put<Policy>(base(id), input),
    remove: (id: string) => http.delete(base(id)),
    updateLimits: (id: string, limits: PolicyLimits) => http.put<Policy>(`${base(id)}/limits`, limits),

    // Ciclo de aprovação
    submit: (id: string) => http.post<Policy>(`${base(id)}/submit`),
    approve: (id: string, approvalRecord: string) => http.post<Policy>(`${base(id)}/approve`, { approvalRecord }),
    openVersion: (id: string, input: { version: string; reason: string | null }) =>
      http.post<Policy>(`${base(id)}/versions`, input),

    // Partes da política
    addAxis: (id: string, axis: PolicyAxisInput) => http.post<Policy>(`${base(id)}/axes`, axis),
    updateAxis: (id: string, axisId: string, axis: PolicyAxisInput) => http.put<Policy>(`${base(id)}/axes/${axisId}`, axis),
    removeAxis: (id: string, axisId: string) => http.delete<Policy>(`${base(id)}/axes/${axisId}`),
    addBand: (id: string, band: CoverageBandInput) => http.post<Policy>(`${base(id)}/bands`, band),
    updateBand: (id: string, bandId: string, band: CoverageBandInput) => http.put<Policy>(`${base(id)}/bands/${bandId}`, band),
    removeBand: (id: string, bandId: string) => http.delete<Policy>(`${base(id)}/bands/${bandId}`),
    addInstrument: (id: string, instrument: PolicyInstrumentInput) => http.post<Policy>(`${base(id)}/instruments`, instrument),
    updateInstrument: (id: string, instrumentId: string, instrument: PolicyInstrumentInput) =>
      http.put<Policy>(`${base(id)}/instruments/${instrumentId}`, instrument),
    removeInstrument: (id: string, instrumentId: string) => http.delete<Policy>(`${base(id)}/instruments/${instrumentId}`),
  };
}
