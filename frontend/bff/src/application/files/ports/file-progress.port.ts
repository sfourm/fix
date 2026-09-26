import type { FileProgressDto } from '../dtos/file-progress.dto.js';

export type FileProgressListener = (event: FileProgressDto) => void;

/** Eventos de progresso do processamento (file.progress no RabbitMQ). */
export interface FileProgressPort {
  /** Devolve a função que cancela a inscrição. */
  subscribe(listener: FileProgressListener): () => void;
}
