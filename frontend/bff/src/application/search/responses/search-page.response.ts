import type { Page } from '../../../cross-cutting/paging/page.js';

/** Página de itens do conjunto (mesmo formato das listagens: OrderResponse, MandateResponse...). */
export interface SearchPageResponse<T = unknown> extends Page<T> {
  truncated: boolean;
  cached: boolean;
}
