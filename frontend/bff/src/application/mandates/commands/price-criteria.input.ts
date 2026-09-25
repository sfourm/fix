export interface PriceCriteriaInput {
  /** A mercado (sem preço-alvo). */
  atMarket: boolean;
  target: number | null;
  min: number | null;
  max: number | null;
  /** Ex.: c/lb, R$/t, R$/US$. */
  unit: string | null;
}
