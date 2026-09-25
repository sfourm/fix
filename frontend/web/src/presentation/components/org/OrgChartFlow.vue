<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue';
import type { Group } from '@/domain/organization';

/**
 * Organograma como fluxo: cartões de cima (raiz) para baixo, ligados por curvas. Com edição, segura-se um grupo e
 * solta-se sobre o grupo que passa a ficar acima dele; durante o arraste a árvore já se reorganiza (prévia ao vivo,
 * como no dashboard) e o lugar onde ele vai cair aparece tracejado. Clique simples abre o grupo.
 */
const props = defineProps<{
  groups: Group[];
  canEdit: boolean;
  memberName: (id: string) => string;
  ruleName: (code: string) => string;
}>();
const emit = defineEmits<{ move: [groupId: string, parentId: string]; open: [groupId: string]; addChild: [parentId: string] }>();

const NODE_W = 212;
const NODE_H = 92;
const GAP_X = 28;
const GAP_Y = 76;
const PAD = 28;

// ---------- Arraste ----------
interface Drag {
  id: string;
  pointerId: number;
  startX: number;
  startY: number;
  offsetX: number;
  offsetY: number;
  x: number;
  y: number;
  active: boolean;
  /** Grupo que ficará acima (prévia). */
  targetId: string | null;
  /** Onde o ponteiro estava quando a prévia mudou (só troca de novo se o usuário mexer). */
  lastChangeX: number;
  lastChangeY: number;
  hoverId: string | null;
  hoverSince: number;
}
const drag = ref<Drag | null>(null);

const byId = computed(() => new Map(props.groups.map((g) => [g.id, g])));

function descendants(list: Group[], id: string): Set<string> {
  const out = new Set<string>();
  const walk = (pid: string) =>
    list.filter((g) => g.parentGroupId === pid).forEach((g) => {
      out.add(g.id);
      walk(g.id);
    });
  walk(id);
  return out;
}

const blocked = computed(() => (drag.value ? new Set([drag.value.id, ...descendants(props.groups, drag.value.id)]) : new Set<string>()));

/** Grupos como ficariam se soltasse agora. */
const effective = computed<Group[]>(() => {
  const d = drag.value;
  if (!d?.active || !d.targetId) return props.groups;
  return props.groups.map((g) => (g.id === d.id ? { ...g, parentGroupId: d.targetId } : g));
});

// ---------- Layout em árvore ----------
const layout = computed(() => {
  const list = effective.value;
  const children = (pid: string | null) =>
    list.filter((g) => g.parentGroupId === pid).sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'));
  const pos = new Map<string, { x: number; y: number; depth: number }>();
  let cursor = 0;
  let maxDepth = 0;
  const place = (g: Group, depth: number) => {
    maxDepth = Math.max(maxDepth, depth);
    const kids = children(g.id);
    let x: number;
    if (!kids.length) {
      x = cursor * (NODE_W + GAP_X);
      cursor++;
    } else {
      kids.forEach((k) => place(k, depth + 1));
      x = (pos.get(kids[0]!.id)!.x + pos.get(kids[kids.length - 1]!.id)!.x) / 2 - PAD;
    }
    pos.set(g.id, { x: x + PAD, y: depth * (NODE_H + GAP_Y) + PAD, depth });
  };
  children(null).forEach((r) => place(r, 0));
  return {
    pos,
    width: Math.max(cursor, 1) * (NODE_W + GAP_X) - GAP_X + PAD * 2,
    height: (maxDepth + 1) * (NODE_H + GAP_Y) - GAP_Y + PAD * 2,
  };
});

const edges = computed(() =>
  effective.value
    .filter((g) => g.parentGroupId && layout.value.pos.has(g.id) && layout.value.pos.has(g.parentGroupId))
    .map((g) => {
      const p = layout.value.pos.get(g.parentGroupId!)!;
      const c = layout.value.pos.get(g.id)!;
      const x1 = p.x + NODE_W / 2;
      const y1 = p.y + NODE_H;
      const x2 = c.x + NODE_W / 2;
      const y2 = c.y;
      const my = (y1 + y2) / 2;
      return {
        id: g.id,
        d: `M ${x1} ${y1} C ${x1} ${my}, ${x2} ${my}, ${x2} ${y2}`,
        preview: drag.value?.active && drag.value.id === g.id,
        dim: drag.value?.active && blocked.value.has(g.id),
      };
    }),
);

// ---------- Zoom e pan ----------
const zoom = ref(1);
const viewport = ref<HTMLElement | null>(null);
const setZoom = (z: number) => (zoom.value = Math.min(1.4, Math.max(0.5, Math.round(z * 10) / 10)));
function fit() {
  const el = viewport.value;
  if (!el) return;
  setZoom(Math.min(1, (el.clientWidth - 8) / layout.value.width));
}

let pan: { x: number; y: number; left: number; top: number } | null = null;
function onBackgroundDown(e: PointerEvent) {
  if (e.button !== 0 || !viewport.value) return;
  pan = { x: e.clientX, y: e.clientY, left: viewport.value.scrollLeft, top: viewport.value.scrollTop };
  window.addEventListener('pointermove', onPan);
  window.addEventListener('pointerup', endPan, { once: true });
}
function onPan(e: PointerEvent) {
  if (!pan || !viewport.value) return;
  viewport.value.scrollLeft = pan.left - (e.clientX - pan.x);
  viewport.value.scrollTop = pan.top - (e.clientY - pan.y);
}
function endPan() {
  pan = null;
  window.removeEventListener('pointermove', onPan);
}

// ---------- Eventos do arraste ----------
function onNodeDown(e: PointerEvent, g: Group) {
  if (e.button !== 0 || (e.target as HTMLElement).closest('button')) return;
  e.stopPropagation();
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
  drag.value = {
    id: g.id,
    pointerId: e.pointerId,
    startX: e.clientX,
    startY: e.clientY,
    offsetX: e.clientX - rect.left,
    offsetY: e.clientY - rect.top,
    x: e.clientX,
    y: e.clientY,
    active: false,
    targetId: null,
    lastChangeX: e.clientX,
    lastChangeY: e.clientY,
    hoverId: null,
    hoverSince: 0,
  };
  window.addEventListener('pointermove', onMove);
  window.addEventListener('pointerup', onUp);
  window.addEventListener('pointercancel', cancel);
  window.addEventListener('keydown', onKey);
}

function nodeAt(x: number, y: number): string | null {
  const el = document.elementFromPoint(x, y)?.closest<HTMLElement>('[data-node]');
  return el?.dataset.node ?? null;
}

let raf = 0;
function autoScroll() {
  const d = drag.value;
  const el = viewport.value;
  if (!d?.active || !el) return;
  const r = el.getBoundingClientRect();
  const edge = 48;
  const speed = 14;
  if (d.x < r.left + edge) el.scrollLeft -= speed;
  else if (d.x > r.right - edge) el.scrollLeft += speed;
  if (d.y < r.top + edge) el.scrollTop -= speed;
  else if (d.y > r.bottom - edge) el.scrollTop += speed;
  raf = requestAnimationFrame(autoScroll);
}

function onMove(e: PointerEvent) {
  const d = drag.value;
  if (!d || e.pointerId !== d.pointerId) return;
  d.x = e.clientX;
  d.y = e.clientY;
  if (!d.active) {
    if (!props.canEdit || !byId.value.get(d.id)?.parentGroupId) return;
    if (Math.hypot(e.clientX - d.startX, e.clientY - d.startY) < 6) return;
    d.active = true;
    d.targetId = byId.value.get(d.id)?.parentGroupId ?? null;
    document.body.classList.add('org-dragging');
    raf = requestAnimationFrame(autoScroll);
  }
  // Nova prévia só depois de o ponteiro se mexer e parar um instante sobre outro grupo (sem "piscar" com o reflow).
  const hit = nodeAt(e.clientX, e.clientY);
  const valid = hit && !blocked.value.has(hit) ? hit : null;
  if (valid !== d.hoverId) {
    d.hoverId = valid;
    d.hoverSince = performance.now();
  }
  const moved = Math.hypot(e.clientX - d.lastChangeX, e.clientY - d.lastChangeY) > 10;
  if (valid && valid !== d.targetId && moved && performance.now() - d.hoverSince > 110) {
    d.targetId = valid;
    d.lastChangeX = e.clientX;
    d.lastChangeY = e.clientY;
  }
}

function finish() {
  cancelAnimationFrame(raf);
  document.body.classList.remove('org-dragging');
  window.removeEventListener('pointermove', onMove);
  window.removeEventListener('pointerup', onUp);
  window.removeEventListener('pointercancel', cancel);
  window.removeEventListener('keydown', onKey);
}

function onUp(e: PointerEvent) {
  const d = drag.value;
  if (!d || e.pointerId !== d.pointerId) return;
  finish();
  if (!d.active) emit('open', d.id);
  else {
    const current = byId.value.get(d.id)?.parentGroupId;
    if (d.targetId && d.targetId !== current) emit('move', d.id, d.targetId);
  }
  drag.value = null;
}

function cancel() {
  finish();
  drag.value = null;
}

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') cancel();
}

onBeforeUnmount(finish);

const dragged = computed(() => (drag.value?.active ? byId.value.get(drag.value.id) ?? null : null));
const initials = (id: string) =>
  props
    .memberName(id)
    .split(/\s+/)
    .slice(0, 2)
    .map((p) => p.charAt(0).toUpperCase())
    .join('');
</script>

<template>
  <div class="flow">
    <div class="toolbar">
      <span class="hint muted small">
        {{ canEdit ? 'Segure um grupo e solte sobre o grupo que ficará acima dele. Clique para abrir.' : 'Clique num grupo para abrir.' }}
      </span>
      <div class="zoom" role="group" aria-label="Zoom">
        <button type="button" aria-label="Diminuir zoom" @click="setZoom(zoom - 0.1)">−</button>
        <button type="button" class="pct" title="Ajustar à largura" @click="fit">{{ Math.round(zoom * 100) }}%</button>
        <button type="button" aria-label="Aumentar zoom" @click="setZoom(zoom + 0.1)">+</button>
      </div>
    </div>

    <div ref="viewport" class="viewport" :class="{ dragging: drag?.active }" @pointerdown="onBackgroundDown">
      <div class="sizer" :style="{ width: `${layout.width * zoom}px`, height: `${layout.height * zoom}px` }">
        <div class="stage" :style="{ width: `${layout.width}px`, height: `${layout.height}px`, transform: `scale(${zoom})` }">
          <svg class="edges" :width="layout.width" :height="layout.height" aria-hidden="true">
            <path v-for="e in edges" :key="e.id" :d="e.d" :class="{ preview: e.preview, dim: e.dim }" />
          </svg>

          <div
            v-for="g in effective"
            :key="g.id"
            class="node"
            :data-node="g.id"
            :class="{
              root: !g.parentGroupId,
              placeholder: drag?.active && drag.id === g.id,
              target: drag?.active && drag.targetId === g.id,
              dim: drag?.active && blocked.has(g.id) && drag.id !== g.id,
              draggable: canEdit && !!g.parentGroupId,
            }"
            :style="{ transform: `translate(${layout.pos.get(g.id)?.x ?? 0}px, ${layout.pos.get(g.id)?.y ?? 0}px)`, width: `${NODE_W}px`, height: `${NODE_H}px` }"
            role="button"
            tabindex="0"
            :aria-label="`Grupo ${g.name}`"
            @pointerdown="onNodeDown($event, g)"
            @keydown.enter="emit('open', g.id)"
          >
            <div class="node-head">
              <strong :title="g.name">{{ g.name }}</strong>
              <span v-if="!g.parentGroupId" class="badge badge-primary">raiz</span>
            </div>
            <div class="node-meta">
              <span class="avatars">
                <span v-for="m in g.memberIds.slice(0, 4)" :key="m" class="av" :title="memberName(m)">{{ initials(m) }}</span>
                <span v-if="g.memberIds.length > 4" class="av more">+{{ g.memberIds.length - 4 }}</span>
                <span v-if="!g.memberIds.length" class="muted small">sem membros</span>
              </span>
              <span class="rules small" :title="g.rules.map(ruleName).join(', ')">{{ g.rules.length }} cargo(s)</span>
            </div>
            <button v-if="canEdit && !drag?.active" class="add" type="button" :title="`Novo grupo abaixo de ${g.name}`" :aria-label="`Novo grupo abaixo de ${g.name}`" @click.stop="emit('addChild', g.id)">+</button>
          </div>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <div
        v-if="dragged && drag"
        class="org-float"
        :style="{ width: `${NODE_W * zoom}px`, transform: `translate(${drag.x - drag.offsetX}px, ${drag.y - drag.offsetY}px) rotate(1.5deg)` }"
        aria-hidden="true"
      >
        <strong>{{ dragged.name }}</strong>
        <span>{{ drag.targetId && drag.targetId !== dragged.parentGroupId ? `abaixo de ${byId.get(drag.targetId)?.name}` : 'solte sobre outro grupo' }}</span>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.flow {
  display: flex;
  flex-direction: column;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  padding: 0 22px 12px;
}

.zoom {
  display: inline-flex;
  gap: 2px;
  padding: 3px;
  border-radius: 999px;
  background: var(--fill);
}

.zoom button {
  min-width: 30px;
  height: 26px;
  padding: 0 8px;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: var(--text);
  font: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
}

.zoom button:hover {
  background: var(--surface-raised);
}

.zoom .pct {
  min-width: 54px;
  font-variant-numeric: tabular-nums;
}

.viewport {
  position: relative;
  max-height: 620px;
  overflow: auto;
  border-top: 1px solid var(--border);
  border-radius: 0 0 var(--radius) var(--radius);
  background:
    radial-gradient(circle, var(--fill-strong) 1px, transparent 1.2px) 0 0 / 22px 22px,
    var(--surface-2);
  cursor: grab;
  touch-action: none;
  user-select: none;
}

.viewport:active {
  cursor: grabbing;
}

.sizer {
  position: relative;
  margin: 0 auto;
}

.stage {
  position: absolute;
  top: 0;
  left: 0;
  transform-origin: 0 0;
}

.edges {
  position: absolute;
  inset: 0;
  overflow: visible;
}

.edges path {
  fill: none;
  stroke: var(--border-strong);
  stroke-width: 2;
  transition:
    d 0.38s var(--ease),
    opacity 0.2s;
}

.edges path.preview {
  stroke: var(--primary);
  stroke-dasharray: 6 5;
}

.edges path.dim {
  opacity: 0.35;
}

.node {
  position: absolute;
  top: 0;
  left: 0;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: 16px;
  background: var(--surface);
  box-shadow: var(--shadow);
  cursor: pointer;
  transition:
    transform 0.38s var(--ease),
    box-shadow 0.25s var(--ease),
    border-color 0.2s,
    opacity 0.2s,
    background 0.2s;
}

.node.draggable {
  cursor: grab;
}

.node:hover {
  box-shadow: var(--shadow-hover);
}

.node:focus-visible {
  outline: none;
  box-shadow: 0 0 0 4px var(--primary-soft), var(--shadow);
}

.node.root {
  box-shadow: inset 0 3px 0 var(--primary), var(--shadow);
}

/* Lugar onde o grupo vai cair: contorno tracejado dourado (igual ao dashboard). */
.node.placeholder {
  border: 2px dashed var(--primary);
  background: var(--primary-soft);
  box-shadow: none;
  opacity: 0.55;
}

.node.target {
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-soft), var(--shadow-hover);
}

.node.dim {
  opacity: 0.4;
}

.node-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  min-width: 0;
}

.node-head strong {
  overflow: hidden;
  font-size: 0.95rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.node-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.avatars {
  display: flex;
  align-items: center;
}

.av {
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;
  margin-right: -6px;
  border: 2px solid var(--surface);
  border-radius: 50%;
  background: linear-gradient(145deg, var(--steel), var(--bronze));
  color: #fff;
  font-size: 0.62rem;
  font-weight: 700;
}

.av.more {
  background: var(--fill-strong);
  color: var(--text-dim);
}

.rules {
  color: var(--text-muted);
  white-space: nowrap;
}

.add {
  position: absolute;
  bottom: -13px;
  left: 50%;
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;
  margin-left: -13px;
  border: 0;
  border-radius: 50%;
  background: var(--primary);
  color: var(--on-primary);
  font-size: 1rem;
  font-weight: 600;
  line-height: 1;
  box-shadow: var(--shadow);
  cursor: pointer;
  opacity: 0;
  transform: scale(0.7);
  transition:
    opacity 0.2s,
    transform 0.2s var(--ease);
}

.node:hover .add,
.node:focus-within .add {
  opacity: 1;
  transform: none;
}

@media (hover: none) {
  .add {
    opacity: 1;
    transform: none;
  }
}

.org-float {
  position: fixed;
  top: 0;
  left: 0;
  z-index: 100;
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 14px 16px;
  border: 1px solid var(--primary-line);
  border-radius: 16px;
  background: var(--surface-raised);
  box-shadow: var(--shadow-lg);
  pointer-events: none;
}

.org-float strong {
  font-size: 0.95rem;
}

.org-float span {
  color: var(--primary);
  font-size: 0.8rem;
  font-weight: 600;
}

@media (max-width: 720px) {
  .toolbar {
    padding: 0 16px 10px;
  }

  .viewport {
    max-height: 70vh;
  }
}
</style>

<style>
body.org-dragging,
body.org-dragging * {
  cursor: grabbing !important;
  user-select: none;
}
</style>
