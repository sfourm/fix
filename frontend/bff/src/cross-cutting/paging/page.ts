export interface PageRequest {
  page: number;
  pageSize: number;
}

export interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export function mapPage<TIn, TOut>(page: Page<TIn>, map: (item: TIn) => TOut): Page<TOut> {
  return { ...page, items: page.items.map(map) };
}
