/**
 * Lista suspensa moderna para todo <select class="input"> do portal, sem trocar os componentes:
 * o <select> nativo continua sendo a fonte da verdade (v-model, :value, validação); só a lista aberta é nossa.
 * Clique ou Enter/Espaço/Alt+↓ abrem a lista; ↑/↓ navegam, Enter escolhe, Esc fecha, digitar busca.
 * Listas longas ganham um campo de busca. Em telas de toque fica o seletor nativo do sistema.
 */

interface Item {
  index: number;
  label: string;
  group: string | null;
  disabled: boolean;
}

let current: { select: HTMLSelectElement; root: HTMLDivElement; items: Item[]; active: number; filter: string } | null = null;

const touch = () => window.matchMedia('(hover: none) and (pointer: coarse)').matches;
const enhanced = (el: EventTarget | null): el is HTMLSelectElement =>
  el instanceof HTMLSelectElement && el.classList.contains('input') && !el.multiple && !el.disabled && el.size <= 1;

function itemsOf(select: HTMLSelectElement): Item[] {
  return [...select.options].map((o) => ({
    index: o.index,
    label: o.label || o.text,
    group: o.parentElement instanceof HTMLOptGroupElement ? o.parentElement.label : null,
    disabled: o.disabled || (o.parentElement instanceof HTMLOptGroupElement && o.parentElement.disabled),
  }));
}

function close(focusSelect = true) {
  if (!current) return;
  const { select, root } = current;
  root.classList.add('closing');
  setTimeout(() => root.remove(), 120);
  select.removeAttribute('aria-expanded');
  current = null;
  window.removeEventListener('resize', onViewportChange);
  window.removeEventListener('scroll', onViewportChange, true);
  if (focusSelect) select.focus({ preventScroll: true });
}

function choose(item: Item) {
  if (!current || item.disabled) return;
  const { select } = current;
  if (select.selectedIndex !== item.index) {
    // selectedIndex (e não value): o v-model do Vue lê o valor real da opção (inclusive null/objetos).
    select.selectedIndex = item.index;
    select.dispatchEvent(new Event('input', { bubbles: true }));
    select.dispatchEvent(new Event('change', { bubbles: true }));
  }
  close();
}

function visible(): Item[] {
  if (!current) return [];
  const f = current.filter.trim().toLocaleLowerCase('pt-BR');
  return f ? current.items.filter((i) => i.label.toLocaleLowerCase('pt-BR').includes(f)) : current.items;
}

function render() {
  if (!current) return;
  const { root, select } = current;
  const list = root.querySelector<HTMLUListElement>('.sp-list')!;
  list.innerHTML = '';
  const shown = visible();
  let lastGroup: string | null = null;
  shown.forEach((item) => {
    if (item.group && item.group !== lastGroup) {
      const g = document.createElement('li');
      g.className = 'sp-group';
      g.textContent = item.group;
      g.setAttribute('role', 'presentation');
      list.appendChild(g);
    }
    lastGroup = item.group;
    const li = document.createElement('li');
    li.className = 'sp-option';
    li.setAttribute('role', 'option');
    li.id = `sp-opt-${item.index}`;
    li.dataset.index = String(item.index);
    const selected = item.index === select.selectedIndex;
    li.setAttribute('aria-selected', String(selected));
    if (item.disabled) li.setAttribute('aria-disabled', 'true');
    if (item.index === current!.active) li.classList.add('active');
    const check = document.createElement('span');
    check.className = 'sp-check';
    check.setAttribute('aria-hidden', 'true');
    if (selected) check.innerHTML = '<svg viewBox="0 0 24 24"><path d="M5 12.5l4.5 4.5L19 7.5" /></svg>';
    const text = document.createElement('span');
    text.className = 'sp-label';
    text.textContent = item.label || '—';
    li.append(check, text);
    li.onpointerenter = () => setActive(item.index, false);
    li.onclick = () => choose(item);
    list.appendChild(li);
  });
  if (!shown.length) {
    const empty = document.createElement('li');
    empty.className = 'sp-empty';
    empty.textContent = 'Nenhuma opção encontrada';
    list.appendChild(empty);
  }
  list.setAttribute('aria-activedescendant', `sp-opt-${current.active}`);
}

function setActive(index: number, scroll = true) {
  if (!current) return;
  current.active = index;
  current.root.querySelectorAll('.sp-option').forEach((el) => el.classList.toggle('active', (el as HTMLElement).dataset.index === String(index)));
  if (scroll) current.root.querySelector(`#sp-opt-${index}`)?.scrollIntoView({ block: 'nearest' });
}

function move(delta: number) {
  const shown = visible().filter((i) => !i.disabled);
  if (!shown.length || !current) return;
  const pos = shown.findIndex((i) => i.index === current!.active);
  const next = shown[Math.min(shown.length - 1, Math.max(0, (pos < 0 ? (delta > 0 ? -1 : shown.length) : pos) + delta))]!;
  setActive(next.index);
}

function position() {
  if (!current) return;
  const { select, root } = current;
  const r = select.getBoundingClientRect();
  const below = window.innerHeight - r.bottom - 12;
  const above = r.top - 12;
  const want = Math.min(340, root.scrollHeight);
  const up = below < Math.min(want, 220) && above > below;
  root.style.left = `${Math.max(8, Math.min(r.left, window.innerWidth - Math.max(r.width, 200) - 8))}px`;
  root.style.minWidth = `${Math.max(r.width, 200)}px`;
  root.style.maxWidth = `${Math.max(r.width, 360)}px`;
  root.style.maxHeight = `${Math.max(160, Math.min(340, up ? above : below))}px`;
  root.style.top = up ? '' : `${r.bottom + 6}px`;
  root.style.bottom = up ? `${window.innerHeight - r.top + 6}px` : '';
  root.classList.toggle('up', up);
}

function onViewportChange(e: Event) {
  // Rolagem dentro da própria lista não fecha.
  if (current && e.type === 'scroll' && current.root.contains(e.target as Node)) return;
  if (!current) return;
  const r = current.select.getBoundingClientRect();
  if (r.bottom < 0 || r.top > window.innerHeight) close(false);
  else position();
}

function open(select: HTMLSelectElement) {
  if (current?.select === select) return close();
  close(false);
  const items = itemsOf(select);
  const root = document.createElement('div');
  root.className = 'sp-popover';
  root.setAttribute('role', 'presentation');
  const searchable = items.length > 10;
  if (searchable) {
    const search = document.createElement('input');
    search.className = 'sp-search';
    search.type = 'search';
    search.placeholder = 'Buscar…';
    search.setAttribute('aria-label', 'Buscar opção');
    search.oninput = () => {
      if (!current) return;
      current.filter = search.value;
      const first = visible().find((i) => !i.disabled);
      current.active = first?.index ?? -1;
      render();
    };
    search.onkeydown = onKey;
    root.appendChild(search);
  }
  const list = document.createElement('ul');
  list.className = 'sp-list';
  list.setAttribute('role', 'listbox');
  const label = select.getAttribute('aria-label') ?? (select.id ? document.querySelector(`label[for="${select.id}"]`)?.textContent : null);
  if (label) list.setAttribute('aria-label', label.trim());
  root.appendChild(list);
  document.body.appendChild(root);

  current = { select, root, items, active: select.selectedIndex, filter: '' };
  select.setAttribute('aria-expanded', 'true');
  render();
  position();
  setActive(select.selectedIndex);
  if (searchable) root.querySelector<HTMLInputElement>('.sp-search')!.focus({ preventScroll: true });
  window.addEventListener('resize', onViewportChange);
  window.addEventListener('scroll', onViewportChange, true);
}

let typed = '';
let typedAt = 0;
function onKey(e: KeyboardEvent) {
  if (!current) return;
  if (e.key === 'ArrowDown') move(1);
  else if (e.key === 'ArrowUp') move(-1);
  else if (e.key === 'Home') move(-Infinity);
  else if (e.key === 'End') move(Infinity);
  else if (e.key === 'Enter') {
    const item = current.items.find((i) => i.index === current!.active);
    if (item) choose(item);
  } else if (e.key === 'Escape') close();
  else if (e.key === 'Tab') close(false);
  else if (e.key.length === 1 && !(e.target instanceof HTMLInputElement)) {
    // Digitar pula para a opção que começa com o texto.
    const now = performance.now();
    typed = now - typedAt > 700 ? e.key : typed + e.key;
    typedAt = now;
    const hit = current.items.find((i) => !i.disabled && i.label.toLocaleLowerCase('pt-BR').startsWith(typed.toLocaleLowerCase('pt-BR')));
    if (hit) setActive(hit.index);
  } else return;
  if (e.key !== 'Tab') e.preventDefault();
  e.stopPropagation();
}

export function installSelectPopover() {
  document.addEventListener(
    'mousedown',
    (e) => {
      if (current && !current.root.contains(e.target as Node) && e.target !== current.select) close(false);
      if (!enhanced(e.target) || e.button !== 0 || touch()) return;
      e.preventDefault();
      e.target.focus({ preventScroll: true });
      open(e.target);
    },
    true,
  );
  document.addEventListener(
    'keydown',
    (e) => {
      if (current) {
        if (e.target === current.select || current.root.contains(e.target as Node)) onKey(e);
        return;
      }
      if (!enhanced(e.target) || touch()) return;
      if (e.key === ' ' || e.key === 'Enter' || (e.altKey && e.key === 'ArrowDown') || e.key === 'F4') {
        e.preventDefault();
        open(e.target);
      }
    },
    true,
  );
  // Troca de rota ou o <select> sumir da tela: fecha.
  new MutationObserver(() => current && !current.select.isConnected && close(false)).observe(document.body, { childList: true, subtree: true });
}
