export const FILE_STATUSES = ['Received', 'Processing', 'Completed', 'CompletedWithErrors', 'Failed', 'Stored'] as const;

export type FileStatus = (typeof FILE_STATUSES)[number];
