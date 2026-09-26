import { EventEmitter } from 'node:events';
import amqp from 'amqplib';
import type { FileProgressDto } from '../../application/files/dtos/file-progress.dto.js';
import type { FileProgressListener, FileProgressPort } from '../../application/files/ports/file-progress.port.js';

const ROUTING_KEY = 'file.progress';
const EVENT = 'progress';

/**
 * Consome o file.progress do storage-service numa fila exclusiva desta instância (cada BFF recebe todos os eventos)
 * e repassa aos navegadores conectados por SSE. Reconecta sozinho se o broker cair.
 */
export class RabbitMqFileProgress implements FileProgressPort {
  private readonly emitter = new EventEmitter();

  private constructor(private readonly model: amqp.RecoveringChannelModel) {
    // Um listener por navegador conectado.
    this.emitter.setMaxListeners(0);
  }

  /** Conecta ao RabbitMQ; sem broker o BFF segue sem acompanhamento ao vivo (a tela recarrega sob demanda). */
  static async connect(url: string, exchange: string): Promise<RabbitMqFileProgress | null> {
    let instance: RabbitMqFileProgress | null = null;
    try {
      const model = await amqp.connect(url, {
        recovery: {
          maxDelay: 10_000,
          setup: async (connection: amqp.ChannelModel) => {
            const channel = await connection.createChannel();
            await channel.assertExchange(exchange, 'topic', { durable: true });
            const { queue } = await channel.assertQueue('', { exclusive: true, autoDelete: true });
            await channel.bindQueue(queue, exchange, ROUTING_KEY);
            await channel.consume(queue, (message) => message && instance?.dispatch(message.content), { noAck: true });
          },
        },
      });
      model.on('error', (error: Error) => console.warn(`RabbitMQ: ${error.message}`));
      instance = new RabbitMqFileProgress(model);
      return instance;
    } catch (error) {
      console.warn(`RabbitMQ indisponível (${(error as Error).message}); uploads sem progresso ao vivo.`);
      return null;
    }
  }

  subscribe(listener: FileProgressListener): () => void {
    this.emitter.on(EVENT, listener);
    return () => this.emitter.off(EVENT, listener);
  }

  close(): Promise<void> {
    return this.model.close();
  }

  private dispatch(content: Buffer): void {
    try {
      this.emitter.emit(EVENT, JSON.parse(content.toString('utf8')) as FileProgressDto);
    } catch {
      // Mensagem ilegível: descartada (o progresso é só informativo; o estado real está no storage).
    }
  }
}
