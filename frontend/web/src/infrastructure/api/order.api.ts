import type { ApprovalStatus, ConfirmationStatus, Order, OrderTermsInput } from '@/domain/order';
import type { Page } from '@/domain/page';
import type { HttpClient } from '../http/http-client';
import type { PageQuery } from './page-query';

export interface OrderQuery extends PageQuery {
  mandateId?: string;
  approval?: ApprovalStatus;
  confirmation?: ConfirmationStatus;
  /** Só boletas sem mandato. */
  withoutMandate?: boolean;
  /** Só boletas FORA do enquadramento. */
  onlyOutside?: boolean;
}

export function createOrderApi(http: HttpClient) {
  return {
    list: ({ withoutMandate, onlyOutside, ...query }: OrderQuery) =>
      http.get<Page<Order>>('/orders', { ...query, withoutMandate: withoutMandate ? 'true' : undefined, onlyOutside: onlyOutside ? 'true' : undefined }),
    get: (id: string) => http.get<Order>(`/orders/${id}`),
    /** mandateId nulo = boleta sem mandato (exige justificativa). */
    register: (input: { mandateId: string | null; counterpartyId: string; terms: OrderTermsInput }) => http.post<Order>('/orders', input),
    update: (id: string, input: { counterpartyId: string; terms: OrderTermsInput }) => http.put<Order>(`/orders/${id}`, input),
    /** Vínculo a posteriori: carimbo permanente. */
    link: (id: string, mandateId: string, justification: string) => http.post<Order>(`/orders/${id}/link`, { mandateId, justification }),
    remove: (id: string) => http.delete(`/orders/${id}`),
    approve: (id: string, note: string | null) => http.post<Order>(`/orders/${id}/approve`, { note }),
    reject: (id: string, note: string) => http.post<Order>(`/orders/${id}/reject`, { note }),

    // Confirmação (middle office)
    confirm: (id: string, receivedOn: string) => http.post<Order>(`/orders/${id}/confirmation`, { receivedOn }),
    markDivergent: (id: string, note: string) => http.post<Order>(`/orders/${id}/divergence`, { note }),
    refuse: (id: string, note: string) => http.post<Order>(`/orders/${id}/refusal`, { note }),
    resolve: (id: string) => http.post<Order>(`/orders/${id}/resolve`),
  };
}
