import type { CounterpartyType } from '../../../cross-cutting/enums/counterparty-type.js';

export interface CounterpartyDto {
  id: string;
  name: string;
  type: CounterpartyType;
  document: string | null;
  address: string | null;
  country: string | null;
  isHomologated: boolean;
  notionalLimitUsd: number | null;
  mtmLimitUsd: number | null;
}
