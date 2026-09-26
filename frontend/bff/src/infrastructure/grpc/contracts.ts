import { readdirSync } from 'node:fs';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { GrpcReflection } from 'grpc-js-reflection-client';
import type { Env } from '../config/env.js';

export const CORE_PACKAGE = 'fix.v1';

export const CORE_SERVICES = [
  'AuthService',
  'OrganizationService',
  'CounterpartyService',
  'PolicyService',
  'MandateService',
  'OrderService',
  'RoleService',
  'RuleService',
  'TimelineService',
] as const;

export type CoreServiceName = (typeof CORE_SERVICES)[number];

/** Serviços do storage-service (arquivos). */
export const STORAGE_SERVICES = ['FileService'] as const;

export type StorageServiceName = (typeof STORAGE_SERVICES)[number];

export type CoreServiceConstructors = Record<CoreServiceName, grpc.ServiceClientConstructor>;

export type ServiceConstructors<T extends string> = Record<T, grpc.ServiceClientConstructor>;

/** Opções de desserialização: camelCase, enums como string e campos default preenchidos. */
const loaderOptions: protoLoader.Options = {
  keepCase: false,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true,
};

/**
 * Carrega os contratos do core. Em "files" lê os .proto da raiz do repositório;
 * em "reflection" pergunta ao próprio core (server reflection), sem precisar dos arquivos.
 */
export async function loadContracts(env: Env, credentials: grpc.ChannelCredentials): Promise<CoreServiceConstructors> {
  return env.GRPC_CONTRACTS === 'reflection'
    ? loadFromReflection(env.CORE_GRPC_URL, credentials, CORE_SERVICES)
    : loadFromFiles(env.PROTOS_DIR, CORE_SERVICES);
}

/** Contratos do storage-service, pelo mesmo modo (arquivos ou reflection no próprio storage). */
export async function loadStorageContracts(
  env: Env,
  credentials: grpc.ChannelCredentials,
): Promise<ServiceConstructors<StorageServiceName>> {
  return env.GRPC_CONTRACTS === 'reflection'
    ? loadFromReflection(env.STORAGE_GRPC_URL, credentials, STORAGE_SERVICES)
    : loadFromFiles(env.PROTOS_DIR, STORAGE_SERVICES);
}

function loadFromFiles<T extends string>(protosDir: string, names: readonly T[]): ServiceConstructors<T> {
  const files = readdirSync(protosDir).filter((file) => file.endsWith('.proto'));
  const definition = protoLoader.loadSync(files, { ...loaderOptions, includeDirs: [protosDir] });
  return pickServices(grpc.loadPackageDefinition(definition), names);
}

async function loadFromReflection<T extends string>(
  url: string,
  credentials: grpc.ChannelCredentials,
  names: readonly T[],
): Promise<ServiceConstructors<T>> {
  const reflection = new GrpcReflection(url, credentials);
  const available = await reflection.listServices();

  const entries = await Promise.all(
    names.map(async (name) => {
      const symbol = `${CORE_PACKAGE}.${name}`;
      if (!available.includes(symbol)) {
        throw new Error(`Serviço ${symbol} não encontrado via reflection em ${url}.`);
      }

      const descriptor = await reflection.getDescriptorBySymbol(symbol);
      const packageObject = descriptor.getPackageObject(loaderOptions);
      return [name, pickServices(packageObject, [name])[name]] as const;
    }),
  );

  return Object.fromEntries(entries) as ServiceConstructors<T>;
}

function pickServices<T extends string>(root: grpc.GrpcObject, names: readonly T[]): ServiceConstructors<T> {
  const pkg = CORE_PACKAGE.split('.').reduce<grpc.GrpcObject | undefined>(
    (node, key) => node?.[key] as grpc.GrpcObject | undefined,
    root,
  );

  const services = {} as ServiceConstructors<T>;
  for (const name of names) {
    const ctor = pkg?.[name];
    if (typeof ctor === 'function') {
      services[name] = ctor as grpc.ServiceClientConstructor;
    }
  }

  return services;
}
