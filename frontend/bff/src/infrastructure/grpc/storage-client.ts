import * as grpc from '@grpc/grpc-js';
import type { Env } from '../config/env.js';
import { loadStorageContracts, type StorageServiceName } from './contracts.js';
import { toAppError } from './grpc-errors.js';

type UnaryMethod = (
  request: object,
  options: grpc.CallOptions,
  callback: (error: grpc.ServiceError | null, response: unknown) => void,
) => void;

/** Canal gRPC com o storage-service (uploads). O arquivo trafega inteiro na mensagem, até 20 MB. */
export class StorageClient {
  private static readonly maxMessageBytes = 24 * 1024 * 1024;

  private constructor(
    private readonly clients: Record<StorageServiceName, grpc.Client>,
    private readonly deadlineMs: number,
  ) {}

  static async connect(env: Env): Promise<StorageClient> {
    const credentials = env.CORE_GRPC_TLS ? grpc.credentials.createSsl() : grpc.credentials.createInsecure();
    const constructors = await loadStorageContracts(env, credentials);
    const options = {
      'grpc.max_send_message_length': StorageClient.maxMessageBytes,
      'grpc.max_receive_message_length': StorageClient.maxMessageBytes,
    };

    const clients = Object.fromEntries(
      Object.entries(constructors).map(([name, Ctor]) => [name, new Ctor(env.STORAGE_GRPC_URL, credentials, options)]),
    ) as unknown as Record<StorageServiceName, grpc.Client>;

    return new StorageClient(clients, env.STORAGE_GRPC_DEADLINE_MS);
  }

  call<TResponse>(service: StorageServiceName, method: string, request: object): Promise<TResponse> {
    const client = this.clients[service] as unknown as Record<string, UnaryMethod>;
    const rpc = client[method];
    if (typeof rpc !== 'function') {
      return Promise.reject(new Error(`RPC ${service}.${method} não existe no contrato.`));
    }

    return new Promise<TResponse>((resolve, reject) => {
      rpc.call(client, request, { deadline: Date.now() + this.deadlineMs }, (error, response) => {
        if (error) {
          reject(toAppError(error, 'de arquivos'));
          return;
        }

        resolve(response as TResponse);
      });
    });
  }

  close(): void {
    for (const client of Object.values(this.clients)) {
      client.close();
    }
  }
}
