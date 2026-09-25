using Fix.Domain.AggregateRoots.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class PolicyVersionConfiguration : IEntityTypeConfiguration<PolicyVersion>
{
    public void Configure(EntityTypeBuilder<PolicyVersion> builder)
    {
        builder.ToTable("policy_versions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Version).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.Note).HasMaxLength(300);

        builder.HasIndex(v => new { v.PolicyId, v.Date });
    }
}

