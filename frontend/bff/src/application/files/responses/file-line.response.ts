import type { FileLineStatus } from '../../../cross-cutting/enums/file-line-status.js';

export interface FileLineResponse {
  id: string;
  /** 1 = primeira linha de dados (o cabeçalho não conta). */
  number: number;
  status: FileLineStatus;
  /** Coluna → valor, como veio no arquivo. */
  values: Record<string, string>;
  /** Erro da linha ou resumo do que foi feito. */
  message: string | null;
  /** Código do que foi criado no core (MD-07, HX-0012). */
  resultCode: string | null;
  processedAt: string | null;
}
