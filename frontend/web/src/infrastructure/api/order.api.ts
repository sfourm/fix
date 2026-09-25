import type { ApprovalStatus, ConfirmationStatus, Order, OrderTerms } from '@/domain/order';
import type { Page } from '@/domain/page';
import type { HttpClient } from '../http/http-client';
import type { PageQuery } from './page-query';

export interface OrderQuery extends PageQuery {
  mandateId?: string;
  approval?: ApprovalStatus;
  confirmation?: ConfirmationStatus;
}

export function createOrderApi(http: HttpClient) {
  return {
    list: (query: OrderQuery) => http.get<Page<Order>>('/orders', { ...query }),
    get: (id: string) => http.get<Order>(`/orders/${id}`),
    register: (input: { mandateId: string; counterpartyId: string; terms: OrderTerms }) => http.post<Order>('/orders', input),
    update: (id: string, input: { counterpartyId: string; terms: OrderTerms }) => http.put<Order>(`/orders/${id}`, input),
    remove: (id: string) => http.delete(`/orders/${id}`),
    approve: (id: string, note: string | null) => http.post<Order>(`/orders/${id}/approve`, { note }),
    reject: (id: string, note: string) => http.post<Order>(`/orders/${id}/reject`, { note }),

    // Confirmation (middle office)
    confirm: (id: string, receivedOn: string) => http.post<Order>(`/orders/${id}/confirmation`, { receivedOn }),
    markDivergent: (id: string, note: string) => http.post<Order>(`/orders/${id}/divergence`, { note }),
    refuse: (id: string, note: string) => http.post<Order>(`/orders/${id}/refusal`, { note }),
    resolve: (id: string) => http.post<Order>(`/orders/${id}/resolve`),
  };
}
