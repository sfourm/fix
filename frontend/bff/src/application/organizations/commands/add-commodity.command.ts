import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { MeasurementUnit } from '../../../cross-cutting/enums/measurement-unit.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface AddCommodityCommand {
  context: RequestContext;
  commodity: Commodity;
  capacity: number;
  unit: MeasurementUnit;
  priceReference: string | null;
  currency: string;
  sells: boolean;
}
