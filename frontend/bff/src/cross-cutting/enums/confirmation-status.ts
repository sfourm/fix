export const CONFIRMATION_STATUSES = ['Pending', 'Confirmed', 'Divergent', 'Refused'] as const;

export type ConfirmationStatus = (typeof CONFIRMATION_STATUSES)[number];
