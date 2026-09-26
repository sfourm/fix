using Fix.Contracts.V1;
using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Abstractions.Processing;
using Fix.Storage.Application.Abstractions.Spreadsheets;
using Fix.Storage.Application.Abstractions.Storage;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using Fix.Storage.Infrastructure.CoreService;
using Fix.Storage.Infrastructure.Messaging;
using Fix.Storage.Infrastructure.Options;
using Fix.Storage.Infrastructure.Persistence;
using Fix.Storage.Infrastructure.Persistence.Repositories;
using Fix.Storage.Infrastructure.Spreadsheets;
using Fix.Storage.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fix.Storage.Infrastructure;

public static class DependencyInjection
{
    /// <summary>MongoDB, S3, RabbitMQ (publicação e consumidores), leitores de planilha e clientes gRPC do core.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection("Mongo"));
        services.Configure<S3Options>(configuration.GetSection("S3"));
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<CoreOptions>(configuration.GetSection("Core"));

        // Persistência (MongoDB)
        services.AddSingleton<StorageDbContext>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFileLineRepository, FileLineRepository>();
        services.AddHostedService<DatabaseInitializer>();

        // Arquivos (S3) e leitura das planilhas
        services.AddSingleton<S3ObjectStorage>();
        services.AddSingleton<IObjectStorage>(provider => provider.GetRequiredService<S3ObjectStorage>());
        services.AddSingleton<ITableReader, TableReader>();

        // Mensageria (RabbitMQ)
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddHostedService<FileUploadConsumer>();
        services.AddHostedService<FileLineConsumer>();

        // core-service (gRPC): regras do usuário e execução das linhas
        services.AddMemoryCache();
        AddCoreClient<OrganizationService.OrganizationServiceClient>(services);
        AddCoreClient<RuleService.RuleServiceClient>(services);
        AddCoreClient<PolicyService.PolicyServiceClient>(services);
        AddCoreClient<MandateService.MandateServiceClient>(services);
        AddCoreClient<OrderService.OrderServiceClient>(services);
        AddCoreClient<CounterpartyService.CounterpartyServiceClient>(services);
        services.AddScoped<IRoleResolver, RoleResolver>();
        services.AddScoped<CoreLookups>();
        services.AddScoped<ILineExecutor, CoreLineExecutor>();

        return services;
    }

    private static void AddCoreClient<TClient>(IServiceCollection services)
        where TClient : class =>
        services.AddGrpcClient<TClient>((provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<CoreOptions>>().Value.Address));
}
