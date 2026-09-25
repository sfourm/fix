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
        builder.HasIndex(r => r.Code).IsUnique();

        builder.HasMany(r => r.Roles).WithOne().HasForeignKey(rr => rr.RuleId).OnDelete(DeleteBehavior.Cascade);

        builder.HasData(SystemRules.All.Select(r => new
        {
            Id = SystemRules.Id(r.Code),
            r.Code,
            r.Name,
            SeedData.CreatedAt,
        }));
    }
}

