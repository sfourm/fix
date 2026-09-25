using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policies");
        builder.HasKey(p => p.Id);
        builder.Ignore(p => p.IsEditable);

        builder.Property(p => p.Code).HasMaxLength(30).IsRequired();
        builder.Property(p => p.Title).HasConversion(ValueObjectConversions.Title).HasMaxLength(Title.MaxLength).IsRequired();
        builder.Property(p => p.Version).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(DomainGuard.DescriptionMaxLength);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ApprovalRecord).HasMaxLength(100);

        builder.ComplexProperty(p => p.Validity, validity =>
        {
            validity.Property(v => v.StartsOn).HasColumnName("valid_from");
            validity.Property(v => v.EndsOn).HasColumnName("valid_to");
        });

        builder.ComplexProperty(p => p.Limits, limits =>
        {
            limits.Property(l => l.AbsoluteCeilingPct).HasPrecision(6, 2);
            limits.Property(l => l.FxFixedMinPct).HasPrecision(6, 2);
            limits.Property(l => l.FxFixedMaxPct).HasPrecision(6, 2);
            limits.Property(l => l.FxUnfixedMaxPct).HasPrecision(6, 2);
            limits.Property(l => l.MarginCashMaxPct).HasPrecision(6, 2);
            limits.Property(l => l.PhysicalConcentrationMaxPct).HasPrecision(6, 2);
            limits.Property(l => l.FinancialConcentrationMaxPct).HasPrecision(6, 2);
            limits.Property(l => l.FreightCeilingPct).HasPrecision(6, 2);
            limits.Property(l => l.CoveredCallMaxPct).HasPrecision(6, 2);
        });

        builder.HasIndex(p => new { p.OrganizationId, p.Code }).IsUnique();
        builder.HasIndex(p => new { p.OrganizationId, p.Status });
        builder.HasOne<Organization>().WithMany().HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Axes).WithOne().HasForeignKey(a => a.PolicyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Bands).WithOne().HasForeignKey(b => b.PolicyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Instruments).WithOne().HasForeignKey(i => i.PolicyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Versions).WithOne().HasForeignKey(v => v.PolicyId).OnDelete(DeleteBehavior.Cascade);
    }
}

