export const APPROVAL_STATUSES = ['PendingApproval', 'Approved', 'Rejected'] as const;

export type ApprovalStatus = (typeof APPROVAL_STATUSES)[number];
