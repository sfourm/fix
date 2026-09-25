using Fix.Domain.AggregateRoots.Rules;
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
    /// Aplica as migrations, garante a role de plataforma super_administrador no Identity
    /// e, se configurado (Seed:SuperAdministrator), cria o usuário super administrador.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));

        await provider.GetRequiredService<FixDbContext>().Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        if (!await roleManager.RoleExistsAsync(RuleCodes.SuperAdministrador))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(RuleCodes.SuperAdministrador) { Id = Guid.CreateVersion7() });
        }

        var seed = provider.GetRequiredService<IConfiguration>().GetSection("Seed:SuperAdministrator");
        var email = seed["Email"];
        var password = seed["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
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
            return;
        }

        await userManager.AddToRoleAsync(user, RuleCodes.SuperAdministrador);
        logger.LogInformation("Super administrador {Email} criado.", email);
    }
}

