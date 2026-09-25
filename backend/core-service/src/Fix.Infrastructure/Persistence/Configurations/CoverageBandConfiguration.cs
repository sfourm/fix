using Fix.Domain.AggregateRoots.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class CoverageBandConfiguration : IEntityTypeConfiguration<CoverageBand>
{
    public void Configure(EntityTypeBuilder<CoverageBand> builder)
    {
        builder.ToTable("policy_coverage_bands");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Horizon).HasMaxLength(60).IsRequired();
        builder.Property(b => b.Crop).HasConversion(ValueObjectConversions.CropYear).HasMaxLength(5).IsRequired();
        builder.Property(b => b.MinPct).HasPrecision(6, 2);
        builder.Property(b => b.MaxPct).HasPrecision(6, 2);
        builder.Property(b => b.Note).HasMaxLength(300);

        builder.HasIndex(b => new { b.PolicyId, b.Crop }).IsUnique();
    }
}

