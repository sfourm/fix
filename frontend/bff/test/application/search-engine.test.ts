import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { aggregate } from '../../src/application/search/engine/aggregator.js';
import { compileCriteria, validateCriteria } from '../../src/application/search/engine/criteria-engine.js';
import { AppError } from '../../src/cross-cutting/errors/app-error.js';

// Linhas no formato OrderResponse (só os campos que o catálogo lê).
const order = (type: string, lots: number, approval: string, counterpartyName: string, tradeDate: string) => ({
  approval,
  confirmation: 'Pending',
  commodity: 'RawSugar',
  counterpartyName,
  mandateTitle: 'M',
  mandateId: 'm1',
  counterpartyId: 'c1',
  confirmationOverdue: false,
  terms: { type, direction: 'Sell', optionKind: null, tenor: 'N27', tradeDate, lots, notionalUsd: null, price: 16, premium: null },
});

const rows = [
  order('Futures', 200, 'Approved', 'Louis Dreyfus', '2026-08-10'),
  order('Option', 50, 'PendingApproval', 'BTG', '2026-09-02'),
  order('Futures', 30, 'Approved', 'Cargill', '2026-09-20'),
  order('Ndf', 0, 'Rejected', 'Itaú', '2026-09-21'),
];

describe('motor de critérios', () => {
  it('rejeita campo inexistente, operador incompatível e valor de enum desconhecido', () => {
    for (const criteria of [
      [{ field: 'nope', operator: 'eq' as const, value: 'x' }],
      [{ field: 'approval', operator: 'gt' as const, value: 'x' }],
      [{ field: 'approval', operator: 'eq' as const, value: 'Talvez' }],
    ]) {
      assert.throws(() => validateCriteria('orders', criteria), AppError);
    }
  });

  it('combina condições com E, texto sem diferenciar maiúsculas', () => {
    const matches = compileCriteria('orders', [
      { field: 'approval', operator: 'eq', value: 'Approved' },
      { field: 'counterpartyName', operator: 'contains', value: 'LOUIS' },
    ]);
    assert.equal(rows.filter(matches).length, 1);

    const between = compileCriteria('orders', [{ field: 'tradeDate', operator: 'between', value: ['2026-09-01', '2026-09-30'] }]);
    assert.equal(rows.filter(between).length, 3);
  });
});

describe('agregação', () => {
  it('soma por categoria com rótulos pt-BR, ordenando pelo valor', () => {
    const r = aggregate('orders', rows, { aggregation: 'sum', measureField: 'lots', groupBy: 'type', sort: 'value-desc', limit: 8 });
    assert.equal(r.total, 280);
    assert.deepEqual(r.points.map((p) => [p.label, p.value]), [['Futuro', 230], ['Opção', 50], ['NDF', 0]]);
  });

  it('dobra o excedente em "Outros" e agrupa por mês em ordem cronológica', () => {
    const top = aggregate('orders', rows, { aggregation: 'count', measureField: null, groupBy: 'counterpartyName', sort: 'value-desc', limit: 2 });
    assert.equal(top.points.length, 2);
    assert.equal(top.points[1]!.label, 'Outros (3)');
    assert.equal(top.points[1]!.value, 3);

    const months = aggregate('orders', rows, { aggregation: 'count', measureField: null, groupBy: 'tradeMonth', sort: 'label', limit: 12 });
    assert.deepEqual(months.points.map((p) => [p.label, p.value]), [['ago/2026', 1], ['set/2026', 3]]);
  });

  it('KPI sem dimensão devolve só o total; média ignora vazios', () => {
    const kpi = aggregate('orders', rows, { aggregation: 'avg', measureField: 'lots', groupBy: null, sort: 'value-desc', limit: 8 });
    assert.equal(kpi.total, 70);
    assert.equal(kpi.points.length, 0);
  });
});
