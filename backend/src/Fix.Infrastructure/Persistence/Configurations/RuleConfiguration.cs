using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class RuleConfiguration : IEntityTypeConfiguration<Rule>
{
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        builder.ToTable("rules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).HasMaxLength(64).IsRequired();
        builder.Property(r => r.Name).HasMaxLength(128).IsRequired();
        // Código único por escopo: entre as rules de sistema (organização nula) e dentro de cada organização.
        builder.HasIndex(r => new { r.OrganizationId, r.Code }).IsUnique().AreNullsDistinct(false);
        builder.HasOne<Organization>().WithMany().HasForeignKey(r => r.OrganizationId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Roles).WithOne().HasForeignKey(rr => rr.RuleId).OnDelete(DeleteBehavior.Cascade);

        builder.HasData(SystemRules.All.Select(r => new
        {
            Id = SystemRules.Id(r.Code),
            r.Code,
            r.Name,
            OrganizationId = (Guid?)null,
            SeedData.CreatedAt,
        }));
    }
}

