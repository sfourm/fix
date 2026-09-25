import type { InstrumentPermission } from '../../../cross-cutting/enums/instrument-permission.js';

export interface PolicyInstrumentInput {
  name: string;
  permission: InstrumentPermission;
  condition: string | null;
}
