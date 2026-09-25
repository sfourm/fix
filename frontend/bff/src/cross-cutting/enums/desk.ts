export const DESKS = ['ExecutionDesk', 'Commercial', 'Logistics', 'Board', 'RiskControl'] as const;

export type Desk = (typeof DESKS)[number];
