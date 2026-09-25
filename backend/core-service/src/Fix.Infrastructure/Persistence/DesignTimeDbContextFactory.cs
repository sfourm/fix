using Fix.Application.Abstractions.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fix.Infrastructure.Persistence;

/// <summary>Permite rodar "dotnet ef migrations" sem subir a API.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FixDbContext>
{
    public FixDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FixDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=fix;Username=fix;Password=fix")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new FixDbContext(options, new NoRequestContext());
    }

    private sealed class NoRequestContext : IRequestContext
    {
        public Guid? UserId => null;

        public Guid? OrganizationId => null;
    }
}
