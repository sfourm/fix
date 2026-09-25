export interface CoverageBandInput {
  horizon: string;
  /** Ano-safra AA/AA. */
  crop: string;
  minPct: number;
  maxPct: number;
  note: string | null;
}
