import { onBeforeUnmount, onMounted, ref, type Ref } from 'vue';
import type { ValueFormat } from '@/domain/search';

export const SERIES_SLOTS = 8;

/** Cor padrão do slot i: variável CSS do tema (clara/escura), em ordem fixa, nunca gerada. */
export const defaultColor = (index: number) => `var(--series-${(index % SERIES_SLOTS) + 1})`;

/** Mesmas cores da paleta padrão (tema claro) em hex, para o seletor de cores do editor. */
export const DEFAULT_PALETTE = ['#2a78d6', '#eb6834', '#1baf7a', '#eda100', '#e87ba4', '#008300', '#4a3aa7', '#e34948'];

/** Paletas prontas para o usuário partir delas (vazia = paleta padrão do tema). */
export const PALETTE_PRESETS: { name: string; palette: string[] }[] = [
  { name: 'Padrão do tema', palette: [] },
  { name: 'Categórica', palette: DEFAULT_PALETTE },
  { name: 'Azul', palette: ['#2a78d6'] },
  { name: 'Verde', palette: ['#1baf7a'] },
  { name: 'Laranja', palette: ['#eb6834'] },
  { name: 'Violeta', palette: ['#4a3aa7'] },
  { name: 'Tons de azul', palette: ['#104281', '#1c5cab', '#2a78d6', '#5598e7', '#86b6ef', '#b7d3f6'] },
];

/**
 * Resolve a cor da marca: modo "single" pinta tudo com a primeira cor; "category" usa uma cor por
 * categoria na ordem da paleta (sem paleta, a do tema). "Outros" fica sempre neutro.
 */
export function colorResolver(mode: 'single' | 'category', palette: string[]) {
  return (index: number, key?: string): string => {
    if (key === '__others__') return 'var(--chart-axis)';
    const slot = mode === 'single' ? 0 : index;
    return palette.length > 0 ? palette[slot % palette.length]! : defaultColor(slot);
  };
}

const integer = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 0 });
const decimal = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 });
const compact = new Intl.NumberFormat('pt-BR', { notation: 'compact', maximumFractionDigits: 1 });
const usd = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

export function formatValue(value: number | null | undefined, format: ValueFormat, short = false): string {
  if (value === null || value === undefined) return '—';
  if (short && Math.abs(value) >= 10_000) return format === 'usd' ? `US$ ${compact.format(value)}` : compact.format(value);
  switch (format) {
    case 'integer':
      return integer.format(value);
    case 'usd':
      return usd.format(value);
    case 'percent':
      return `${decimal.format(value)}%`;
    default:
      return decimal.format(value);
  }
}

/** "Bonitos" limites de eixo: 0 até um múltiplo redondo acima do máximo, com ~4 marcas. */
export function niceTicks(max: number, min = 0, count = 4): number[] {
  const span = Math.max(max - min, 1e-9);
  const raw = span / count;
  const magnitude = 10 ** Math.floor(Math.log10(raw));
  const step = [1, 2, 2.5, 5, 10].map((m) => m * magnitude).find((s) => s >= raw) ?? raw;
  const start = Math.floor(min / step) * step;
  // A última marca precisa cobrir o máximo (senão a marca de dados sai da área do gráfico).
  const ticks: number[] = [];
  for (let v = start; ; v += step) {
    ticks.push(Number(v.toFixed(10)));
    if (v >= max - 1e-9) break;
  }
  return ticks;
}

export const truncate = (text: string, max: number) => (text.length > max ? `${text.slice(0, max - 1)}…` : text);

/** Tamanho do elemento (o SVG desenha em pixels reais para manter traços finos e textos nítidos). */
export function useElementSize(el: Ref<HTMLElement | null>) {
  const width = ref(0);
  const height = ref(0);
  let observer: ResizeObserver | null = null;

  onMounted(() => {
    observer = new ResizeObserver(([entry]) => {
      width.value = Math.floor(entry!.contentRect.width);
      height.value = Math.floor(entry!.contentRect.height);
    });
    if (el.value) observer.observe(el.value);
  });
  onBeforeUnmount(() => observer?.disconnect());

  return { width, height };
}

/** Caminho de barra com cantos arredondados só na ponta de dados (a base fica reta, ancorada no eixo). */
export function barPath(x: number, y: number, w: number, h: number, direction: 'up' | 'right', r = 4): string {
  if (w <= 0 || h <= 0) return '';
  if (direction === 'up') {
    const rr = Math.min(r, w / 2, h);
    return `M${x},${y + h}V${y + rr}Q${x},${y} ${x + rr},${y}H${x + w - rr}Q${x + w},${y} ${x + w},${y + rr}V${y + h}Z`;
  }

  const rr = Math.min(r, h / 2, w);
  return `M${x},${y}H${x + w - rr}Q${x + w},${y} ${x + w},${y + rr}V${y + h - rr}Q${x + w},${y + h} ${x + w - rr},${y + h}H${x}Z`;
}
