export const COMPLIANCE_STATUSES = ['Within', 'Outside'] as const;

export type ComplianceStatus = (typeof COMPLIANCE_STATUSES)[number];
