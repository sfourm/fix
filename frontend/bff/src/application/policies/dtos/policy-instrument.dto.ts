import type { InstrumentPermission } from '../../../cross-cutting/enums/instrument-permission.js';

export interface PolicyInstrumentDto {
  id: string;
  name: string;
  permission: InstrumentPermission;
  condition: string | null;
}
