import type { MeasurementUnit } from '../../../cross-cutting/enums/measurement-unit.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateCommodityCommand {
  context: RequestContext;
  commodityId: string;
  capacity: number;
  unit: MeasurementUnit;
  priceReference: string | null;
  currency: string;
  sells: boolean;
}
