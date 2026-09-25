import type { CounterpartyType } from '../../../cross-cutting/enums/counterparty-type.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateCounterpartyCommand {
  context: RequestContext;
  id: string;
  name: string;
  type: CounterpartyType;
  /** CNPJ/CPF ou registro no exterior. */
  document: string | null;
  address: string | null;
  country: string | null;
  /** Limite de nocional (US$). */
  notionalLimitUsd: number | null;
  /** Limite de exposição MtM (US$). */
  mtmLimitUsd: number | null;
}
