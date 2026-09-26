import type { CounterpartyType } from '../../../cross-cutting/enums/counterparty-type.js';

export interface CounterpartyDto {
  id: string;
  /** Código legível da organização (CP-01). */
  code: string;
  name: string;
  type: CounterpartyType;
  document: string | null;
  address: string | null;
  country: string | null;
  isHomologated: boolean;
  notionalLimitUsd: number | null;
  mtmLimitUsd: number | null;
}
