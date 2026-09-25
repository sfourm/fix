export interface CoverageBandResponse {
  id: string;
  horizon: string;
  /** Ano-safra AA/AA. */
  crop: string;
  minPct: number;
  maxPct: number;
  note: string | null;
}
