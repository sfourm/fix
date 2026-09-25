import type { Counterparty, CounterpartyInput } from '@/domain/counterparty';
import type { HttpClient } from '../http/http-client';

export function createCounterpartyApi(http: HttpClient) {
  return {
    list: (onlyHomologated = false) => http.get<Counterparty[]>('/counterparties', { onlyHomologated: String(onlyHomologated) }),
    get: (id: string) => http.get<Counterparty>(`/counterparties/${id}`),
    create: (input: CounterpartyInput) => http.post<Counterparty>('/counterparties', input),
    update: (id: string, input: CounterpartyInput) => http.put<Counterparty>(`/counterparties/${id}`, input),
    setHomologation: (id: string, homologated: boolean) =>
      http.patch<Counterparty>(`/counterparties/${id}/homologation`, { homologated }),
    remove: (id: string) => http.delete(`/counterparties/${id}`),
  };
}
