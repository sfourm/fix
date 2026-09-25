import { getNodeAutoInstrumentations } from '@opentelemetry/auto-instrumentations-node';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-http';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';
import { NodeSDK } from '@opentelemetry/sdk-node';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions';

const isTelemetryEnabled = process.env.OTEL_ENABLED !== 'false';

let sdk: NodeSDK | null = null;

if (isTelemetryEnabled) {
  const rawEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT || 'http://localhost:4318';
  const cleanEndpoint = rawEndpoint.replace(/\/$/, '');
  const serviceName = process.env.OTEL_SERVICE_NAME || 'fix-bff';

  const traceUrl = cleanEndpoint.endsWith('/v1/traces') ? cleanEndpoint : `${cleanEndpoint}/v1/traces`;
  const metricUrl = cleanEndpoint.endsWith('/v1/metrics') ? cleanEndpoint : `${cleanEndpoint}/v1/metrics`;

  const traceExporter = new OTLPTraceExporter({ url: traceUrl });
  const metricExporter = new OTLPMetricExporter({ url: metricUrl });

  sdk = new NodeSDK({
    resource: resourceFromAttributes({
      [ATTR_SERVICE_NAME]: serviceName,
      [ATTR_SERVICE_VERSION]: '1.0.0',
      environment: process.env.NODE_ENV || 'development',
    }),
    traceExporter,
    metricReader: new PeriodicExportingMetricReader({
      exporter: metricExporter,
      exportIntervalMillis: 10_000,
    }),
    instrumentations: [
      getNodeAutoInstrumentations({
        '@opentelemetry/instrumentation-fs': { enabled: false },
        '@opentelemetry/instrumentation-http': { enabled: true },
        '@opentelemetry/instrumentation-express': { enabled: true },
        '@opentelemetry/instrumentation-grpc': { enabled: true },
        '@opentelemetry/instrumentation-redis': { enabled: true },
      }),
    ],
  });

  sdk.start();
}

export async function shutdownTelemetry(): Promise<void> {
  if (sdk) {
    await sdk.shutdown();
  }
}

export { sdk };
