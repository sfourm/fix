export interface PolicyLimitsInput {
  /** Horizonte máximo de hedge (anos). */
  hedgeHorizonYears: number;
  /** Teto absoluto de cobertura (% da produção). */
  absoluteCeilingPct: number;
  fxFixedMinPct: number;
  fxFixedMaxPct: number;
  fxUnfixedMaxPct: number;
  /** Chamada de margem máxima (% do caixa). */
  marginCashMaxPct: number;
  physicalConcentrationMaxPct: number;
  financialConcentrationMaxPct: number;
  logisticsDeadlineMonths: number;
  freightCeilingPct: number;
  coveredCallMaxPct: number;
}
