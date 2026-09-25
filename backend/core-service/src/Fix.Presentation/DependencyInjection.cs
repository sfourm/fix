using Fix.Presentation.Interceptors;
using Fix.Presentation.Services;

namespace Fix.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionInterceptor>();
            options.EnableDetailedErrors = false;
        });
        services.AddGrpcReflection();

        return services;
    }

    public static WebApplication MapPresentation(this WebApplication app)
    {
        app.MapGrpcService<AuthGrpcService>();
        app.MapGrpcService<OrganizationGrpcService>();
        app.MapGrpcService<PolicyGrpcService>();
        app.MapGrpcService<MandateGrpcService>();
        app.MapGrpcService<OrderGrpcService>();
        app.MapGrpcService<CounterpartyGrpcService>();
        app.MapGrpcService<RoleGrpcService>();
        app.MapGrpcService<RuleGrpcService>();
        app.MapGrpcService<TimelineGrpcService>();

        if (app.Configuration.GetValue("Grpc:EnableReflection", true))
        {
            app.MapGrpcReflectionService();
        }

        app.MapGet("/", () => "Fix Core API (gRPC). Use um cliente gRPC; os contratos são expostos via server reflection.");

        return app;
    }
}
