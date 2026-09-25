using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);
        builder.Ignore(o => o.Quantity);
        builder.Ignore(o => o.ConsumedQuantity);
        builder.Ignore(o => o.CanBeDeleted);

        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Direction).HasConversion<string>().HasMaxLength(10);
        builder.Property(o => o.Commodity).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.Tenor).HasConversion(ValueObjectConversions.Tenor).HasMaxLength(10).IsRequired();
        builder.Property(o => o.Lots).HasPrecision(18, 2);
        builder.Property(o => o.NotionalUsd).HasPrecision(18, 2);
        builder.Property(o => o.Price).HasPrecision(18, 6);
        builder.Property(o => o.PriceUnit).HasMaxLength(20).IsRequired();
        builder.Property(o => o.OptionKind).HasConversion<string>().HasMaxLength(10);
        builder.Property(o => o.Premium).HasPrecision(18, 6);
        builder.Property(o => o.Notes).HasMaxLength(DomainGuard.DescriptionMaxLength);
        builder.Property(o => o.Approval).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.DecisionNote).HasMaxLength(500);
        builder.Property(o => o.Confirmation).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.ConfirmationNote).HasMaxLength(500);

        builder.HasIndex(o => new { o.OrganizationId, o.MandateId, o.Approval });
        builder.HasIndex(o => new { o.OrganizationId, o.Confirmation });
        builder.HasOne<Organization>().WithMany().HasForeignKey(o => o.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Mandate>().WithMany().HasForeignKey(o => o.MandateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Counterparty>().WithMany().HasForeignKey(o => o.CounterpartyId).OnDelete(DeleteBehavior.Restrict);
    }
}

