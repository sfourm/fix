import { createRequire } from 'node:module';

/**
 * Carregado com `--import` antes da aplicação. O BFF é ESM, e pacotes CommonJS importados de ESM não passam pelo hook
 * de `require` das instrumentações automáticas. Pré-carregá-los aqui via `require`, com o SDK já ativo, aplica o patch;
 * o `import` da aplicação reaproveita o mesmo módulo do cache. Sem isso o cliente gRPC fica sem spans e o trace do BFF
 * não continua no core.
 */
const PRELOAD_COMMONJS = ['@grpc/grpc-js'];

await import('./telemetry.js');

const require = createRequire(import.meta.url);
for (const name of PRELOAD_COMMONJS) require(name);
