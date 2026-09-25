using System.Reflection;
using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Fix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IRoleResolver, RoleResolver>();

        services.AddScoped<RequestContext>();
        services.AddScoped<IRequestContext>(provider => provider.GetRequiredService<RequestContext>());

        // Ordem = ordem de execução: contexto -> autorização -> validação -> handler.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestContextBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddClosedImplementations(assembly, typeof(ICommandHandler<,>));
        services.AddClosedImplementations(assembly, typeof(IQueryHandler<,>));
        services.AddClosedImplementations(assembly, typeof(IDomainEventHandler<>));
        services.AddClosedImplementations(assembly, typeof(IValidator<>));

        return services;
    }

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
