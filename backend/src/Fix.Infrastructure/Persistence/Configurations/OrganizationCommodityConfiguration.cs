using Fix.Domain.AggregateRoots.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationCommodityConfiguration : IEntityTypeConfiguration<OrganizationCommodity>
{
    public void Configure(EntityTypeBuilder<OrganizationCommodity> builder)
    {
        builder.ToTable("organization_commodities");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Commodity).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Unit).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Capacity).HasPrecision(18, 2);
        builder.Property(c => c.PriceReference).HasMaxLength(100);
        builder.Property(c => c.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(c => new { c.OrganizationId, c.Commodity }).IsUnique();
    }
}

