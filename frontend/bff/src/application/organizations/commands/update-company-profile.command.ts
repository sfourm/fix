import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Sector } from '../../../cross-cutting/enums/sector.js';

export interface UpdateCompanyProfileCommand {
  context: RequestContext;
  corporateName: string;
  taxId: string | null;
  headquarters: string | null;
  group: string | null;
  sector: Sector;
  /** Mês de início do ano-safra (1–12). */
  cropYearStartMonth: number;
  /** AA/AA */
  activeCrop: string | null;
}
