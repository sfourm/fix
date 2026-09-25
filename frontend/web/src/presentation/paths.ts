/**
 * Caminhos da cadeia 1:N do FIX: toda navegação por mandatos e boletas passa pela política.
 *   /policies/:policyId → /policies/:policyId/mandates/:mandateId → …/orders/:orderId
 */
export const paths = {
  policies: () => '/policies',
  policy: (policyId: string, tab?: 'axes' | 'instruments' | 'mandates' | 'approvals' | 'confirmations' | 'history') =>
    `/policies/${policyId}${tab ? `/${tab}` : ''}`,
  newMandate: (policyId: string) => `/policies/${policyId}/mandates/new`,
  mandate: (policyId: string, mandateId: string) => `/policies/${policyId}/mandates/${mandateId}`,
  newOrder: (policyId: string, mandateId: string) => `/policies/${policyId}/mandates/${mandateId}/orders/new`,
  order: (policyId: string, mandateId: string, orderId: string) => `/policies/${policyId}/mandates/${mandateId}/orders/${orderId}`,
  group: (groupId: string) => `/members/groups/${groupId}`,
};
