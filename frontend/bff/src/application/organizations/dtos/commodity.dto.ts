import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { MeasurementUnit } from '../../../cross-cutting/enums/measurement-unit.js';

export interface CommodityDto {
  id: string;
  commodity: Commodity;
  capacity: number;
  unit: MeasurementUnit;
  priceReference: string | null;
  currency: string;
  sells: boolean;
}
