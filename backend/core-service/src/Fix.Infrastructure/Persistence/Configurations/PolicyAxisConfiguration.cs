using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class PolicyAxisConfiguration : IEntityTypeConfiguration<PolicyAxis>
{
    public void Configure(EntityTypeBuilder<PolicyAxis> builder)
    {
        builder.ToTable("policy_axes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Title).HasConversion(ValueObjectConversions.Title).HasMaxLength(Title.MaxLength).IsRequired();
        builder.Property(a => a.Factor).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Statement).HasMaxLength(500);
        builder.Property(a => a.LimitDescription).HasMaxLength(200);
        builder.Property(a => a.Approver).HasMaxLength(150);
        builder.PrimitiveCollection(a => a.Restrictions)
            .HasField("_restrictions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(a => new { a.PolicyId, a.Code }).IsUnique();
    }
}

