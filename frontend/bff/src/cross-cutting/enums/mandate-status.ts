export const MANDATE_STATUSES = ['PendingApproval', 'Active', 'Rejected', 'Closed'] as const;

export type MandateStatus = (typeof MANDATE_STATUSES)[number];
