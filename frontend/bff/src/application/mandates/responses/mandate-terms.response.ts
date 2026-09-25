import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { MeasurementUnit } from '../../../cross-cutting/enums/measurement-unit.js';
import type { PriceCriteriaResponse } from './price-criteria.response.js';

export interface MandateTermsResponse {
  title: string;
  criteria: string | null;
  /** Obrigatória em mandatos de precificação; opcional nos demais tipos. */
  commodity: Commodity | null;
  /** Tela/vencimento: N26, fev/27. */
  tenor: string | null;
  quantity: number | null;
  quantityUnit: MeasurementUnit;
  price: PriceCriteriaResponse;
  windowStart: string | null;
  windowEnd: string | null;
}
