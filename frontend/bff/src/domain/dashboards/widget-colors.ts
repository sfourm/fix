import { ensure } from '../common/domain-error.js';

export const MAX_COLORS = 12;

/**
 * single: uma cor para todas as marcas (a série é uma só) · category: uma cor por categoria, na ordem
 * das cores informadas. Sem cores, o web usa a paleta padrão (validada para daltonismo e tema escuro).
 */
export const COLOR_MODES = ['single', 'category'] as const;
export type ColorMode = (typeof COLOR_MODES)[number];

export interface WidgetColors {
  readonly mode: ColorMode;
  readonly palette: readonly string[];
}

export function createColors(mode: ColorMode, palette: readonly string[]): WidgetColors {
  ensure(palette.length <= MAX_COLORS, `No máximo ${MAX_COLORS} cores por widget.`);
  const normalized = palette.map((color) => {
    ensure(/^#[0-9a-fA-F]{6}$/.test(color), `Cor inválida: "${color}" (use #RRGGBB).`);
    return color.toLowerCase();
  });
  return Object.freeze({ mode, palette: Object.freeze(normalized) });
}
