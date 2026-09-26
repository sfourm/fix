using System.Reflection;
using Fix.Storage.Application.Abstractions.Validation;
using Fix.Storage.Application.Authorization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Fix.Storage.Application;

public static class DependencyInjection
{
    /// <summary>Namespace das interfaces dos services (casos de uso) expostas às outras camadas.</summary>
    private const string UseCaseInterfacesNamespace = "Fix.Storage.Application.Common.Interfaces.UseCases";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IValidationFactory, ValidationFactory>();
        services.AddScoped<UseCaseGuard>();
        services.AddScoped<FilePermissions>();

        services.AddUseCaseServices(assembly);
        services.AddClosedImplementations(assembly, typeof(IValidator<>));

        return services;
    }

    /// <summary>
    /// Registra cada service da Application pela sua interface (FileService → IFileService), embrulhado pelo
    /// <see cref="UseCaseGuardProxy{TService}"/>, que aplica a autorização antes de cada caso de uso.
    /// </summary>
    private static void AddUseCaseServices(this IServiceCollection services, Assembly assembly)
    {
        var create = typeof(DependencyInjection).GetMethod(nameof(CreateGuarded), BindingFlags.NonPublic | BindingFlags.Static)!;

        var registrations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Service", StringComparison.Ordinal))
            .Select(type => (Implementation: type, Service: type.GetInterface($"I{type.Name}")))
            .Where(pair => pair.Service is not null && pair.Service.Namespace == UseCaseInterfacesNamespace);

        foreach (var (implementation, service) in registrations)
        {
            var factory = create.MakeGenericMethod(service!, implementation);
            services.AddScoped(service!, provider => factory.Invoke(null, [provider])!);
        }
    }

    private static TService CreateGuarded<TService, TImplementation>(IServiceProvider provider)
        where TService : class
        where TImplementation : TService =>
        UseCaseGuardProxy<TService>.Create(
            ActivatorUtilities.CreateInstance<TImplementation>(provider),
            provider.GetRequiredService<UseCaseGuard>());

    private static void AddClosedImplementations(this IServiceCollection services, Assembly assembly, Type openGeneric)
    {
        var registrations = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGeneric)
                .Select(i => (Service: i, Implementation: type)));

        foreach (var (service, implementation) in registrations)
        {
            services.AddScoped(service, implementation);
        }
    }
}
