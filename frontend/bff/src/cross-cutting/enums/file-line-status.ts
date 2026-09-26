export const FILE_LINE_STATUSES = ['Pending', 'Succeeded', 'Failed'] as const;

export type FileLineStatus = (typeof FILE_LINE_STATUSES)[number];
