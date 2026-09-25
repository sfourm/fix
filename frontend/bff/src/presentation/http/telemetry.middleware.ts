import { metrics, trace } from '@opentelemetry/api';
import type { RequestHandler } from 'express';

const meter = metrics.getMeter('fix-bff');

const requestCounter = meter.createCounter('http_requests_total', {
  description: 'Total number of HTTP requests processed by BFF with route, status and method',
});

const requestDuration = meter.createHistogram('http_request_duration_ms', {
  description: 'Duration of HTTP requests in milliseconds',
  unit: 'ms',
});

export const telemetryMiddleware: RequestHandler = (req, res, next) => {
  const start = Date.now();
  const activeSpan = trace.getActiveSpan();
  const traceId = activeSpan?.spanContext().traceId;

  if (traceId) {
    res.setHeader('x-correlation-id', traceId);
    res.setHeader('x-trace-id', traceId);
    activeSpan.setAttribute('correlation.id', traceId);
  }

  res.on('finish', () => {
    const duration = Date.now() - start;
    const method = req.method;
    const statusCode = res.statusCode;
    const statusClass = `${Math.floor(statusCode / 100)}xx`;

    // Constrói o caminho canônico da rota ou usa a URL da requisição
    const matchedPath = req.baseUrl ? `${req.baseUrl}${req.route?.path ?? ''}` : (req.route?.path ?? req.path);
    const route = matchedPath || req.path || 'unknown';

    if (activeSpan) {
      activeSpan.updateName(`${method} ${route}`);
      activeSpan.setAttribute('http.route', route);
      activeSpan.setAttribute('http.status_code', statusCode);
      activeSpan.setAttribute('http.status_class', statusClass);
      if (statusCode >= 400) {
        activeSpan.setAttribute('error', true);
      }
    }

    const labels = {
      method,
      route,
      status_code: String(statusCode),
      status_class: statusClass,
    };

    requestCounter.add(1, labels);
    requestDuration.record(duration, labels);
  });

  next();
};
