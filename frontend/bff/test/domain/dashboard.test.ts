import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import type { Actor } from '../../src/domain/common/actor.js';
import { DomainError } from '../../src/domain/common/domain-error.js';
import { Dashboard } from '../../src/domain/dashboards/dashboard.js';
import type { WidgetDefinition } from '../../src/domain/dashboards/dashboard-widget.js';
import { SavedFilter } from '../../src/domain/filters/saved-filter.js';

const org = 'org-1';
const owner: Actor = { userId: 'u-owner', organizationId: org, canManageShared: false };
const colleague: Actor = { userId: 'u-colleague', organizationId: org, canManageShared: false };
const manager: Actor = { userId: 'u-manager', organizationId: org, canManageShared: true };
const outsider: Actor = { userId: 'u-owner', organizationId: 'org-2', canManageShared: true };

const kpi = (overrides: Partial<WidgetDefinition> = {}): WidgetDefinition => ({
  title: 'Mandatos ativos',
  type: 'Kpi',
  layout: { width: 3, height: 140 },
  query: { source: 'mandates', aggregation: 'count', measureField: null, groupBy: null, filterId: null, criteria: [], sort: 'value-desc', limit: 8 },
  colors: { mode: 'single', palette: [] },
  ...overrides,
});

const bars = (overrides: Partial<WidgetDefinition['query']> = {}): WidgetDefinition =>
  kpi({ title: 'Boletas por instrumento', type: 'Column', layout: { width: 6, height: 300 }, query: { ...kpi().query, source: 'orders', groupBy: 'type', ...overrides } });

const rejects = (fn: () => unknown, kind: 'rule' | 'forbidden' = 'rule') =>
  assert.throws(fn, (error: unknown) => error instanceof DomainError && error.kind === kind);

describe('Dashboard', () => {
  it('nasce com o dono e a organização de quem cria', () => {
    const d = Dashboard.create(owner, { name: '  Mesa  ', description: null, visibility: 'Private', widgets: [kpi()] });
    assert.equal(d.name, 'Mesa');
    assert.equal(d.ownerId, owner.userId);
    assert.equal(d.widgets.length, 1);
  });

  it('privado: só o dono vê; público: a organização vê e só dono/gestor edita', () => {
    const d = Dashboard.create(owner, { name: 'D', description: null, visibility: 'Private' });
    assert.equal(d.canView(colleague), false);
    assert.equal(d.canView(outsider), false);

    d.changeVisibility(owner, 'Public');
    assert.equal(d.canView(colleague), true);
    assert.equal(d.canEdit(colleague), false);
    assert.equal(d.canEdit(manager), true);
    rejects(() => d.addWidget(colleague, kpi()), 'forbidden');
  });

  it('só o dono muda a visibilidade, mesmo que um gestor possa editar', () => {
    const d = Dashboard.create(owner, { name: 'D', description: null, visibility: 'Public' });
    d.rename(manager, 'Renomeado pelo gestor');
    rejects(() => d.changeVisibility(manager, 'Private'), 'forbidden');
  });

  it('valida largura (2–12 colunas), altura (120–720 px) e cores #RRGGBB', () => {
    const d = Dashboard.create(owner, { name: 'D', description: null, visibility: 'Private' });
    rejects(() => d.addWidget(owner, kpi({ layout: { width: 13, height: 200 } })));
    rejects(() => d.addWidget(owner, kpi({ layout: { width: 4, height: 90 } })));
    rejects(() => d.addWidget(owner, kpi({ colors: { mode: 'single', palette: ['red'] } })));

    const w = d.addWidget(owner, kpi({ colors: { mode: 'category', palette: ['#2A78D6'] } }));
    assert.deepEqual(w.colors.palette, ['#2a78d6']);
  });

  it('exige dimensão nos gráficos, proíbe no KPI e exige campo em somas', () => {
    const d = Dashboard.create(owner, { name: 'D', description: null, visibility: 'Private' });
    rejects(() => d.addWidget(owner, bars({ groupBy: null })));
    rejects(() => d.addWidget(owner, kpi({ query: { ...kpi().query, groupBy: 'status' } })));
    rejects(() => d.addWidget(owner, bars({ aggregation: 'sum', measureField: null })));
    rejects(() => d.addWidget(owner, kpi({ type: 'Donut', query: { ...bars().query, limit: 12 } })));
    assert.equal(d.addWidget(owner, bars({ aggregation: 'sum', measureField: 'lots' })).type, 'Column');
  });

  it('atualiza o widget mantendo o id e reordena só com todos os widgets', () => {
    const d = Dashboard.create(owner, { name: 'D', description: null, visibility: 'Private', widgets: [kpi(), bars()] });
    const [first, second] = d.widgets;
    const updated = d.updateWidget(owner, first!.id, kpi({ title: 'Novo título', layout: { width: 12, height: 200 } }));
    assert.equal(updated.id, first!.id);
    assert.equal(d.widgets[0]!.layout.width, 12);

    rejects(() => d.reorderWidgets(owner, [second!.id]));
    d.reorderWidgets(owner, [second!.id, first!.id]);
    assert.deepEqual(d.widgets.map((w) => w.id), [second!.id, first!.id]);
  });

  it('duplica um público como cópia privada de quem duplicou', () => {
    const d = Dashboard.create(owner, { name: 'Org', description: null, visibility: 'Public', widgets: [kpi()] });
    const copy = d.duplicate(colleague);
    assert.equal(copy.ownerId, colleague.userId);
    assert.equal(copy.visibility, 'Private');
    assert.notEqual(copy.widgets[0]!.id, d.widgets[0]!.id);
  });
});

describe('SavedFilter', () => {
  const criteria = [{ field: 'approval', operator: 'eq' as const, value: 'Approved' }];

  it('exige ao menos uma condição e valida a forma de cada operador', () => {
    rejects(() => SavedFilter.create(owner, { name: 'F', source: 'orders', criteria: [], visibility: 'Private' }));
    rejects(() => SavedFilter.create(owner, { name: 'F', source: 'orders', criteria: [{ field: 'lots', operator: 'between', value: [10, 1] }], visibility: 'Private' }));
    rejects(() => SavedFilter.create(owner, { name: 'F', source: 'orders', criteria: [{ field: 'type', operator: 'in', value: [] }], visibility: 'Private' }));

    const f = SavedFilter.create(owner, { name: 'F', source: 'orders', criteria: [...criteria, { field: 'tenor', operator: 'isEmpty', value: 'x' }], visibility: 'Private' });
    assert.equal(f.criteria[1]!.value, null);
  });

  it('segue as mesmas regras de visibilidade dos dashboards', () => {
    const f = SavedFilter.create(owner, { name: 'F', source: 'orders', criteria, visibility: 'Private' });
    assert.equal(f.canView(colleague), false);
    f.changeVisibility(owner, 'Public');
    assert.equal(f.canView(colleague), true);
    rejects(() => f.changeCriteria(colleague, criteria), 'forbidden');
  });
});
