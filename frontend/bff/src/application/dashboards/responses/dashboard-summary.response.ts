import type { Visibility } from '../../../domain/common/visibility.js';

export interface DashboardSummaryResponse {
  id: string;
  name: string;
  description: string | null;
  visibility: Visibility;
  ownerId: string;
  isMine: boolean;
  canEdit: boolean;
  widgetCount: number;
  updatedAt: string;
}
