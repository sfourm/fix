export const POLICY_STATUSES = ['Draft', 'UnderApproval', 'Active', 'Superseded'] as const;

export type PolicyStatus = (typeof POLICY_STATUSES)[number];
