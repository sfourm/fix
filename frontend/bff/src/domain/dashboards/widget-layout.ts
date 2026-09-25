import { ensure } from '../common/domain-error.js';

/** Grade de 12 colunas: a largura é em colunas e a altura em pixels. */
export const GRID_COLUMNS = 12;
export const MIN_HEIGHT = 120;
export const MAX_HEIGHT = 720;

export interface WidgetLayout {
  readonly width: number;
  readonly height: number;
}

export function createLayout(width: number, height: number): WidgetLayout {
  ensure(Number.isInteger(width) && width >= 2 && width <= GRID_COLUMNS, `A largura do widget deve ser de 2 a ${GRID_COLUMNS} colunas.`);
  ensure(Number.isInteger(height) && height >= MIN_HEIGHT && height <= MAX_HEIGHT, `A altura do widget deve ser de ${MIN_HEIGHT} a ${MAX_HEIGHT} px.`);
  return Object.freeze({ width, height });
}
