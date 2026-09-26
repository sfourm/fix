/**
 * Caminhos da cadeia 1:N do FIX: toda navegação por mandatos e boletas passa pela política.
 *   /policies/:policyId → /policies/:policyId/mandates/:mandateId → …/orders/:orderId
 */
export const paths = {
  policies: () => '/policies',
  policy: (policyId: string, tab?: 'axes' | 'instruments' | 'mandates' | 'approvals' | 'confirmations' | 'history' | 'audit') =>
    `/policies/${policyId}${tab ? `/${tab}` : ''}`,
  newMandate: (policyId: string) => `/policies/${policyId}/mandates/new`,
  mandate: (policyId: string, mandateId: string) => `/policies/${policyId}/mandates/${mandateId}`,
  newOrder: (policyId: string, mandateId: string) => `/policies/${policyId}/mandates/${mandateId}/orders/new`,
  order: (policyId: string, mandateId: string, orderId: string) => `/policies/${policyId}/mandates/${mandateId}/orders/${orderId}`,
  group: (groupId: string) => `/members/groups/${groupId}`,
  /** Exceções (FIX2 · I-01): boletas sem mandato ou FORA do enquadramento, fora da cadeia 1:N. */
  exceptions: () => '/exceptions',
  newOrderWithoutMandate: () => '/exceptions/orders/new',
  orderWithoutMandate: (orderId: string) => `/exceptions/orders/${orderId}`,
  /** Caminho da boleta: na cadeia da política quando tem mandato; nas exceções quando não tem. */
  orderOf: (order: { id: string; mandateId: string | null }, policyId?: string) =>
    order.mandateId && policyId ? paths.order(policyId, order.mandateId, order.id) : order.mandateId ? `/orders/${order.id}` : paths.orderWithoutMandate(order.id),
};
