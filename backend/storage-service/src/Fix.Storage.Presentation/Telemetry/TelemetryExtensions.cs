using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Fix.Storage.Presentation.Telemetry;

public static class TelemetryExtensions
{
    public const string DefaultServiceName = "fix-storage";
    public const string DefaultServiceVersion = "1.0.0";

    public static IServiceCollection AddStorageTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue("OpenTelemetry:Enabled", true))
        {
            return services;
        }

        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? DefaultServiceName;
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: DefaultServiceVersion)
                .AddAttributes(
                [
                    new KeyValuePair<string, object>("environment", configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"),
                    new KeyValuePair<string, object>("service.name", serviceName),
                ]))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
                })
                .AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation(options => options.RecordException = true)
                .AddSource(serviceName)
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(serviceName)
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));

        return services;
    }
}
