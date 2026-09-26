export interface PolicyLimitsDto {
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
  /** Contingência escalonada: reserva da produção projetada que nunca é vendida nem fixada (% por prazo). */
  contingency1MonthPct: number;
  contingency6MonthsPct: number;
  contingency12MonthsPct: number;
  contingency24MonthsPct: number;
  contingency36MonthsPct: number;
  /** Recompra: vendido acima deste % do novo disponível, no prazo em dias úteis. */
  buybackTriggerPct: number;
  buybackDeadlineBusinessDays: number;
  /** Estresse de caixa: choque em desvios-padrão sobre o horizonte em dias úteis. */
  stressSigmas: number;
  stressDays: number;
  /** Régua de fixação: percentil da série FG/A. */
  pricingHotPercentile: number;
  pricingColdPercentile: number;
  /** Virada de mix acima destes p.p. exige rito. */
  mixShiftMaxPp: number;
  /** Controles: confirmação (d.u.), registro da boleta (dias; 0 = D+0) e reporte de desvio (horas). */
  confirmationDeadlineBusinessDays: number;
  registrationDeadlineDays: number;
  deviationReportHours: number;
}
