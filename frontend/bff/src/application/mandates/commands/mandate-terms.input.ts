import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { MeasurementUnit } from '../../../cross-cutting/enums/measurement-unit.js';
import type { PriceCriteriaInput } from './price-criteria.input.js';

export interface MandateTermsInput {
  title: string;
  criteria: string | null;
  commodity: Commodity;
  /** Tela/vencimento: N26, fev/27. */
  tenor: string | null;
  quantity: number | null;
  quantityUnit: MeasurementUnit;
  price: PriceCriteriaInput;
  windowStart: string | null;
  windowEnd: string | null;
}
