import { context, metrics, trace } from '@opentelemetry/api';
import { getRPCMetadata, RPCType } from '@opentelemetry/core';
import type { Request, RequestHandler } from 'express';

const meter = metrics.getMeter('fix-bff');

const requestCounter = meter.createCounter('http_requests_total', {
  description: 'Total number of HTTP requests processed by BFF with route, status and method',
});

const requestDuration = meter.createHistogram('http_request_duration_ms', {
  description: 'Duration of HTTP requests in milliseconds',
  unit: 'ms',
  advice: { explicitBucketBoundaries: [5, 10, 25, 50, 75, 100, 250, 500, 750, 1000, 2500, 5000, 10000] },
});

/**
 * Rota canônica (`/api/orders/:id`). Usa a rota completa que a instrumentação do Express registra no RPCMetadata:
 * no `finish` o `req.baseUrl` já pode ter sido restaurado pelo router. Sem rota casada vira `unmatched`,
 * para não explodir a cardinalidade com ids.
 */
function routeOf(req: Request, matched: string | undefined): string {
  if (!req.route?.path) return 'unmatched';
  const route = matched ?? `${req.baseUrl}${req.route.path}`;
  return route.length > 1 ? route.replace(/\/+$/, '') : route;
}

export const telemetryMiddleware: RequestHandler = (req, res, next) => {
  const start = performance.now();
  // O span ativo aqui é o do middleware do Express (termina no next()); o span do request HTTP vem do RPCMetadata.
  const rpc = getRPCMetadata(context.active());
  const serverSpan = rpc?.type === RPCType.HTTP ? rpc.span : trace.getActiveSpan();
  const traceId = serverSpan?.spanContext().traceId;

  if (traceId) {
    res.setHeader('x-correlation-id', traceId);
    res.setHeader('x-trace-id', traceId);
  }

  res.on('finish', () => {
    const method = req.method;
    const statusCode = res.statusCode;
    const statusClass = `${Math.floor(statusCode / 100)}xx`;
    const route = routeOf(req, rpc?.type === RPCType.HTTP ? rpc.route : undefined);

    if (serverSpan) {
      serverSpan.updateName(`${method} ${route}`);
      serverSpan.setAttribute('http.route', route);
      serverSpan.setAttribute('http.status_class', statusClass);
      if (req.organizationId) serverSpan.setAttribute('fix.organization_id', req.organizationId);
    }

    const labels = { method, route, status_code: String(statusCode), status_class: statusClass };
    requestCounter.add(1, labels);
    requestDuration.record(performance.now() - start, labels);
  });

  next();
};
