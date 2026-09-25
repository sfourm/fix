import type { OrderDto } from '../dtos/order.dto.js';
import type { OrderResponse } from '../responses/order.response.js';

export const toOrderResponse = (dto: OrderDto): OrderResponse => ({ ...dto, terms: { ...dto.terms } });
