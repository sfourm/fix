import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Page, PageRequest } from '../../../cross-cutting/paging/page.js';

/** Mensagens do contrato chegam em camelCase pelo proto-loader; os tipos Contract* descrevem só os campos usados. */

export type ContractTimestamp = { seconds: string; nanos: number };

export interface ContractPageInfo {
  page: number;
  pageSize: number;
  totalCount: number;
}

export const toContractContext = (ctx: RequestContext) => ({
  userId: ctx.userId,
  organizationId: ctx.organizationId ?? '',
});

export const toContractPage = (page: PageRequest) => ({ page: page.page, pageSize: page.pageSize });

export const toPage = <TContract, T>(
  items: TContract[],
  info: ContractPageInfo | null,
  map: (item: TContract) => T,
): Page<T> => ({
  items: items.map(map),
  page: info?.page ?? 1,
  pageSize: info?.pageSize ?? items.length,
  totalCount: info?.totalCount ?? items.length,
});

/** Campos proto3 "optional" só são enviados quando há valor. */
export const optional = <K extends string>(key: K, value: string | null | undefined): Partial<Record<K, string>> =>
  value === null || value === undefined || value === '' ? {} : ({ [key]: value } as Record<K, string>);

export const nullable = (value: string | undefined | null): string | null => (value ? value : null);

/** Campos numéricos "optional": ausentes no contrato viram null. */
export const nullableNumber = (value: number | undefined | null): number | null => value ?? null;

export const toIso = (timestamp: ContractTimestamp | null | undefined): string =>
  timestamp ? new Date(Number(timestamp.seconds) * 1000 + Math.floor(timestamp.nanos / 1e6)).toISOString() : '';
