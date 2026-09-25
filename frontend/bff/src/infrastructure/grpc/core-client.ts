import * as grpc from '@grpc/grpc-js';
import type { Env } from '../config/env.js';
import { loadContracts, type CoreServiceName } from './contracts.js';
import { toAppError } from './grpc-errors.js';

type UnaryMethod = (
  request: object,
  options: grpc.CallOptions,
  callback: (error: grpc.ServiceError | null, response: unknown) => void,
) => void;

/** Canal gRPC com o core. Cada serviço do contrato vira um client sobre o mesmo canal. */
export class CoreClient {
  private constructor(
    private readonly clients: Record<CoreServiceName, grpc.Client>,
    private readonly deadlineMs: number,
  ) {}

  static async connect(env: Env): Promise<CoreClient> {
    const credentials = env.CORE_GRPC_TLS ? grpc.credentials.createSsl() : grpc.credentials.createInsecure();
    const constructors = await loadContracts(env, credentials);

    const clients = Object.fromEntries(
      Object.entries(constructors).map(([name, Ctor]) => [name, new Ctor(env.CORE_GRPC_URL, credentials)]),
    ) as unknown as Record<CoreServiceName, grpc.Client>;

    return new CoreClient(clients, env.CORE_GRPC_DEADLINE_MS);
  }

  call<TResponse>(service: CoreServiceName, method: string, request: object): Promise<TResponse> {
    const client = this.clients[service] as unknown as Record<string, UnaryMethod>;
    const rpc = client[method];
    if (typeof rpc !== 'function') {
      return Promise.reject(new Error(`RPC ${service}.${method} não existe no contrato.`));
    }

    return new Promise<TResponse>((resolve, reject) => {
      rpc.call(client, request, { deadline: Date.now() + this.deadlineMs }, (error, response) => {
        if (error) {
          reject(toAppError(error));
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
