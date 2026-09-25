using Fix.Application.Abstractions.Authentication;
using Fix.Application.Abstractions.Timeline;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Counterparties.Repositories;
using Fix.Domain.AggregateRoots.Mandates.Repositories;
using Fix.Domain.AggregateRoots.Orders.Repositories;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Policies.Repositories;
using Fix.Domain.AggregateRoots.Roles.Repositories;
using Fix.Domain.AggregateRoots.Rules.Repositories;
using Fix.Infrastructure.Identity;
using Fix.Infrastructure.Persistence;
using Fix.Infrastructure.Persistence.Auditing;
using Fix.Infrastructure.Persistence.Repositories;
using Fix.Infrastructure.Persistence.Telemetry;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database não configurada.");

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditingInterceptor>();

        services.AddDbContext<FixDbContext>((provider, options) => options
            .UseNpgsql(connectionString, npgsql => npgsql.ConfigureDataSource(dataSource => dataSource.ConfigureTracing(SqlCommandTelemetry.Configure)))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(provider.GetRequiredService<AuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IMandateRepository, MandateRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITimelineReader, TimelineReader>();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<FixDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
