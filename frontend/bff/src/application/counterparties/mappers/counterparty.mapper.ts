import type { CounterpartyDto } from '../dtos/counterparty.dto.js';
import type { CounterpartyResponse } from '../responses/counterparty.response.js';

export const toCounterpartyResponse = (dto: CounterpartyDto): CounterpartyResponse => ({ ...dto });
