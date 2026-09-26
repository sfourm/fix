using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Presentation.Interceptors;
using Fix.Storage.Presentation.Services;

namespace Fix.Storage.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionInterceptor>();
            options.EnableDetailedErrors = false;
            // O arquivo trafega inteiro na mensagem (limite do use case + folga do envelope).
            options.MaxReceiveMessageSize = UploadFileCommand.MaxBytes + 1024 * 1024;
        });
        services.AddGrpcReflection();

        return services;
    }

    public static WebApplication MapPresentation(this WebApplication app)
    {
        app.MapGrpcService<FileGrpcService>();

        if (app.Configuration.GetValue("Grpc:EnableReflection", true))
        {
            app.MapGrpcReflectionService();
        }

        app.MapGet("/", () => "Fix Storage (gRPC): envio e processamento de arquivos. Os contratos são expostos via server reflection.");

        return app;
    }
}
