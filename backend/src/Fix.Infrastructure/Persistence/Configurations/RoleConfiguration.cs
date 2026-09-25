using Fix.Domain.AggregateRoots.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).HasMaxLength(64).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(256).IsRequired();
        builder.HasIndex(r => r.Code).IsUnique();

        builder.HasData(SystemRoles.Descriptions.Select(r => new
        {
            Id = SystemRoles.Id(r.Key),
            Code = r.Key,
            Description = r.Value,
            SeedData.CreatedAt,
        }));
    }
}

