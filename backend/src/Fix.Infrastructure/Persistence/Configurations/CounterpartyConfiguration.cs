using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.ToTable("counterparties");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasConversion(ValueObjectConversions.Name).HasMaxLength(Name.MaxLength).IsRequired();
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Document).HasMaxLength(32);
        builder.Property(c => c.Address).HasMaxLength(200);
        builder.Property(c => c.Country).HasMaxLength(2);
        builder.Property(c => c.NotionalLimitUsd).HasPrecision(18, 2);
        builder.Property(c => c.MtmLimitUsd).HasPrecision(18, 2);

        builder.HasIndex(c => new { c.OrganizationId, c.Name }).IsUnique();
        builder.HasOne<Organization>().WithMany().HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}

