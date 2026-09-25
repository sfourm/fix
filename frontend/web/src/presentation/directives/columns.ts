import type { Directive } from 'vue';
import { storage } from '@/infrastructure/storage/local-storage';

/**
 * v-columns="'chave'" numa <table>: o usuário controla as colunas.
 *  - arrasta a borda direita do cabeçalho para alargar/estreitar (duplo clique volta ao automático);
 *  - no botão de colunas (canto do cabeçalho) mostra/oculta colunas e restaura o padrão.
 * A tabela pode ficar mais larga que a tela (rola na horizontal dentro do .table-wrap).
 * Preferências salvas no navegador por chave de tabela, identificando a coluna pelo texto do cabeçalho
 * (colunas condicionais com v-if não bagunçam a ordem). Coluna sem título (ações) não pode ser ocultada.
 */

interface Prefs {
  widths: Record<string, number>;
  hidden: string[];
}

interface State {
  key: string;
  id: string;
  prefs: Prefs;
  style: HTMLStyleElement;
  anchor: HTMLDivElement;
  menu: HTMLDivElement | null;
  observer: MutationObserver;
  onDocDown: (e: PointerEvent) => void;
}

const states = new WeakMap<HTMLTableElement, State>();
let seq = 0;
const MIN_W = 56;

const storageKey = (key: string) => `fix.table.${key}`;
const headers = (table: HTMLTableElement) => [...(table.tHead?.rows[0]?.cells ?? [])] as HTMLTableCellElement[];
/** Texto do cabeçalho sem os elementos que a diretiva põe dentro dele. */
const labelOf = (th: HTMLTableCellElement, i: number) => {
  const text = [...th.childNodes]
    .filter((n) => !(n instanceof HTMLElement && n.classList.contains('col-resize')))
    .map((n) => n.textContent ?? '')
    .join('')
    .trim();
  return text || `#${i}`;
};

function save(s: State) {
  storage.set(storageKey(s.key), s.prefs);
}

/** Oculta colunas por CSS (nth-child), sem mexer no DOM que o Vue controla. */
function applyHidden(table: HTMLTableElement, s: State) {
  const idx = headers(table)
    .map((th, i) => (s.prefs.hidden.includes(labelOf(th, i)) ? i + 1 : 0))
    .filter(Boolean);
  const sel = `table[data-cols='${s.id}']`;
  s.style.textContent = idx
    .map((n) => `${sel} > thead > tr > :nth-child(${n}), ${sel} > tbody > tr:not(:has(> [colspan])) > :nth-child(${n}), ${sel} > tfoot > tr > :nth-child(${n}) { display: none; }`)
    .join('\n');
}

/** Larguras: com alguma coluna ajustada, a tabela passa a layout fixo e cresce para caber a soma. */
function applyWidths(table: HTMLTableElement, s: State) {
  table.querySelector(':scope > colgroup.cols-group')?.remove();
  const ths = headers(table);
  const hasCustom = ths.some((th, i) => s.prefs.widths[labelOf(th, i)]);
  if (!hasCustom) {
    table.style.tableLayout = '';
    table.style.width = '';
    return;
  }
  // Colunas sem largura salva ficam com a largura que têm hoje (medida antes de fixar o layout).
  const visible = ths.map((th, i) => ({ th, label: labelOf(th, i) })).filter((c) => !s.prefs.hidden.includes(c.label));
  const widths = visible.map((c) => s.prefs.widths[c.label] ?? Math.max(MIN_W, Math.round(c.th.getBoundingClientRect().width) || 120));
  const group = document.createElement('colgroup');
  group.className = 'cols-group';
  widths.forEach((w) => {
    const col = document.createElement('col');
    col.style.width = `${w}px`;
    group.appendChild(col);
  });
  table.insertBefore(group, table.firstChild);
  table.style.tableLayout = 'fixed';
  table.style.width = `${widths.reduce((a, b) => a + b, 0)}px`;
}

function startResize(e: PointerEvent, table: HTMLTableElement, th: HTMLTableCellElement, label: string) {
  e.preventDefault();
  e.stopPropagation();
  const s = states.get(table);
  if (!s) return;
  // Congela as larguras atuais de todas as colunas antes do primeiro ajuste.
  headers(table).forEach((h, i) => {
    const l = labelOf(h, i);
    if (!s.prefs.hidden.includes(l) && !s.prefs.widths[l]) s.prefs.widths[l] = Math.round(h.getBoundingClientRect().width);
  });
  const startX = e.clientX;
  const startW = th.getBoundingClientRect().width;
  document.body.classList.add('col-resizing');
  const move = (ev: PointerEvent) => {
    s.prefs.widths[label] = Math.max(MIN_W, Math.round(startW + ev.clientX - startX));
    applyWidths(table, s);
  };
  const up = () => {
    document.body.classList.remove('col-resizing');
    window.removeEventListener('pointermove', move);
    window.removeEventListener('pointerup', up);
    save(s);
  };
  window.addEventListener('pointermove', move);
  window.addEventListener('pointerup', up);
}

/** Alças de redimensionar em cada cabeçalho (recolocadas se o Vue re-renderizar o cabeçalho). */
function decorate(table: HTMLTableElement) {
  const s = states.get(table);
  if (!s) return;
  s.observer.disconnect();
  headers(table).forEach((th, i) => {
    const label = labelOf(th, i);
    let handle = th.querySelector<HTMLSpanElement>(':scope > .col-resize');
    if (!handle) {
      handle = document.createElement('span');
      handle.className = 'col-resize';
      handle.setAttribute('aria-hidden', 'true');
      th.appendChild(handle);
    }
    handle.onpointerdown = (e) => startResize(e, table, th, label);
    handle.ondblclick = (e) => {
      e.stopPropagation();
      delete s.prefs.widths[label];
      if (Object.keys(s.prefs.widths).length <= 1) s.prefs.widths = {};
      save(s);
      applyWidths(table, s);
    };
    handle.onclick = (e) => e.stopPropagation();
  });
  applyHidden(table, s);
  applyWidths(table, s);
  if (table.tHead) s.observer.observe(table.tHead, { childList: true, subtree: true, characterData: true });
}

function closeMenu(s: State) {
  s.menu?.remove();
  s.menu = null;
  document.removeEventListener('pointerdown', s.onDocDown, true);
}

function openMenu(table: HTMLTableElement, s: State) {
  if (s.menu) return closeMenu(s);
  const menu = document.createElement('div');
  menu.className = 'cols-menu';
  menu.setAttribute('role', 'dialog');
  menu.setAttribute('aria-label', 'Colunas da tabela');
  const title = document.createElement('p');
  title.className = 'cols-menu-title';
  title.textContent = 'Colunas';
  menu.appendChild(title);

  headers(table).forEach((th, i) => {
    const label = labelOf(th, i);
    if (label.startsWith('#')) return;
    const row = document.createElement('label');
    const box = document.createElement('input');
    box.type = 'checkbox';
    box.className = 'switch';
    box.checked = !s.prefs.hidden.includes(label);
    box.onchange = () => {
      const visibleCount = headers(table).filter((h, j) => !labelOf(h, j).startsWith('#') && !s.prefs.hidden.includes(labelOf(h, j))).length;
      if (!box.checked && visibleCount <= 1) {
        box.checked = true;
        return;
      }
      s.prefs.hidden = box.checked ? s.prefs.hidden.filter((h) => h !== label) : [...s.prefs.hidden, label];
      save(s);
      applyHidden(table, s);
      applyWidths(table, s);
    };
    const text = document.createElement('span');
    text.textContent = label;
    row.append(text, box);
    menu.appendChild(row);
  });

  const reset = document.createElement('button');
  reset.type = 'button';
  reset.className = 'btn btn-sm btn-block';
  reset.textContent = 'Restaurar padrão';
  reset.onclick = () => {
    s.prefs = { widths: {}, hidden: [] };
    storage.remove(storageKey(s.key));
    applyHidden(table, s);
    applyWidths(table, s);
    closeMenu(s);
  };
  const tip = document.createElement('p');
  tip.className = 'cols-menu-tip';
  tip.textContent = 'Arraste a borda de um cabeçalho para mudar a largura; duplo clique volta ao automático.';
  menu.append(reset, tip);

  s.anchor.appendChild(menu);
  s.menu = menu;
  setTimeout(() => document.addEventListener('pointerdown', s.onDocDown, true));
}

export const vColumns: Directive<HTMLTableElement, string | undefined> = {
  mounted(table, binding) {
    const key = binding.value || `${location.pathname.replace(/[0-9a-f-]{36}/g, ':id')}#${seq}`;
    const id = `c${++seq}`;
    table.dataset.cols = id;
    table.classList.add('cols-ctl');

    const style = document.createElement('style');
    document.head.appendChild(style);

    // Botão de colunas: fica numa âncora antes do .table-wrap (fora da área que rola).
    const wrap = table.closest('.table-wrap') ?? table;
    const anchor = document.createElement('div');
    anchor.className = 'cols-anchor';
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'cols-btn';
    button.title = 'Colunas da tabela';
    button.setAttribute('aria-label', 'Colunas da tabela');
    button.innerHTML =
      '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v14H4zM9.5 5v14M14.5 5v14" /></svg>';
    anchor.appendChild(button);
    wrap.parentElement?.insertBefore(anchor, wrap);

    const state: State = {
      key,
      id,
      prefs: { widths: {}, hidden: [], ...(storage.get<Prefs>(storageKey(key)) ?? {}) },
      style,
      anchor,
      menu: null,
      observer: new MutationObserver(() => decorate(table)),
      onDocDown: (e) => {
        if (!anchor.contains(e.target as Node)) closeMenu(state);
      },
    };
    states.set(table, state);
    button.onclick = () => openMenu(table, state);
    decorate(table);
  },
  updated(table) {
    const s = states.get(table);
    if (s) applyHidden(table, s);
  },
  beforeUnmount(table) {
    const s = states.get(table);
    if (!s) return;
    closeMenu(s);
    s.observer.disconnect();
    s.style.remove();
    s.anchor.remove();
    states.delete(table);
  },
};
