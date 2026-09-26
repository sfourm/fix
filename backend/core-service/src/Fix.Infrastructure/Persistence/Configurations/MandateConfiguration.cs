using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class MandateConfiguration : IEntityTypeConfiguration<Mandate>
{
    public void Configure(EntityTypeBuilder<Mandate> builder)
    {
        builder.ToTable("mandates");
        builder.HasKey(m => m.Id);
        builder.Ignore(m => m.AcceptsOrders);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Title).HasConversion(ValueObjectConversions.Title).HasMaxLength(Title.MaxLength).IsRequired();
        builder.Property(m => m.Criteria).HasMaxLength(DomainGuard.DescriptionMaxLength);
        builder.Property(m => m.Commodity).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Tenor).HasConversion(ValueObjectConversions.OptionalTenor).HasMaxLength(10);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.QuantityUnit).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.DecisionNote).HasMaxLength(500);

        builder.ComplexProperty(m => m.Price, price =>
        {
            price.Ignore(p => p.HasAnyLevel);
            price.Property(p => p.Target).HasPrecision(18, 6);
            price.Property(p => p.Min).HasPrecision(18, 6);
            price.Property(p => p.Max).HasPrecision(18, 6);
            price.Property(p => p.Unit).HasMaxLength(20);
        });

        builder.ComplexProperty(m => m.Compliance, compliance =>
        {
            compliance.Ignore(c => c.IsWithin);
            compliance.Property(c => c.Status).HasConversion<string>().HasMaxLength(10);
            compliance.Property(c => c.Reason).HasMaxLength(500).IsRequired();
        });

        builder.Ignore(m => m.Code);
        builder.HasIndex(m => new { m.OrganizationId, m.Number }).IsUnique();
        builder.HasIndex(m => new { m.OrganizationId, m.Status });
        builder.HasIndex(m => new { m.OrganizationId, m.PolicyId });
        builder.HasOne<Organization>().WithMany().HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Policy>().WithMany().HasForeignKey(m => m.PolicyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PolicyAxis>().WithMany().HasForeignKey(m => m.AxisId).OnDelete(DeleteBehavior.Restrict);
    }
}

