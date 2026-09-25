using Fix.Domain.AggregateRoots.Organizations;
using Fix.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fix.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica as migrations e garante a organização nativa FIX com o super administrador (Seed:SuperAdministrator).
    /// A equipe interna é definida pela membership na organização FIX, não por roles do Identity.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));
        var dbContext = provider.GetRequiredService<FixDbContext>();

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Organizations.AnyAsync(o => o.Id == Organization.InternalOrganizationId))
        {
            return;
        }

        var superAdministrator = await EnsureSuperAdministratorAsync(provider, logger);
        if (superAdministrator is null)
        {
            logger.LogWarning("Organização FIX não criada: configure Seed:SuperAdministrator (Email, Password, FullName).");
            return;
        }

        dbContext.Organizations.Add(Organization.CreateInternal(superAdministrator.Id));
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Organização FIX criada com o super administrador {Email}.", superAdministrator.Email);
    }

    private static async Task<ApplicationUser?> EnsureSuperAdministratorAsync(IServiceProvider provider, ILogger logger)
    {
        var seed = provider.GetRequiredService<IConfiguration>().GetSection("Seed:SuperAdministrator");
        var email = seed["Email"];
        var password = seed["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is { } existing)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            UserName = email,
            FullName = seed["FullName"] ?? "Super Administrador",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Não foi possível criar o super administrador: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return null;
        }

        return user;
    }
}
