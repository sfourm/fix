using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .HasConversion(ValueObjectConversions.Name)
            .HasMaxLength(Name.MaxLength)
            .IsRequired();

        builder.Property(o => o.Slug)
            .HasConversion(ValueObjectConversions.Slug)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();
        builder.HasIndex(o => o.Slug).IsUnique();
        builder.Property(o => o.IsInternal).HasDefaultValue(false);

        builder.ComplexProperty(o => o.Profile, profile =>
        {
            profile.Property(p => p.CorporateName).HasMaxLength(200).IsRequired();
            profile.Property(p => p.TaxId).HasMaxLength(14);
            profile.Property(p => p.Headquarters).HasMaxLength(150);
            profile.Property(p => p.Group).HasMaxLength(150);
            profile.Property(p => p.Sector).HasConversion<string>().HasMaxLength(20);
            profile.Property(p => p.ActiveCrop).HasMaxLength(5);
        });

        builder.ComplexProperty(o => o.Industrial, industrial =>
        {
            industrial.Property(i => i.MillingCapacity).HasPrecision(18, 2);
            industrial.Property(i => i.MixMinPct).HasPrecision(6, 2);
            industrial.Property(i => i.MixMaxPct).HasPrecision(6, 2);
            industrial.Property(i => i.MixGuidancePct).HasPrecision(6, 2);
        });

        builder.ComplexProperty(o => o.Budget, budget =>
        {
            budget.Property(b => b.CashCost).HasPrecision(12, 4);
            budget.Property(b => b.EconomicFloor).HasPrecision(12, 4);
            budget.Property(b => b.EquivalentPrice).HasPrecision(12, 4);
            budget.Property(b => b.TargetMarginPct).HasPrecision(6, 2);
        });

        builder.ComplexProperty(o => o.Financials, financials =>
        {
            financials.Ignore(f => f.Leverage);
            financials.Property(f => f.Cash).HasPrecision(18, 2);
            financials.Property(f => f.CreditLines).HasPrecision(18, 2);
            financials.Property(f => f.MonthlyFixedCost).HasPrecision(18, 2);
            financials.Property(f => f.NetDebt).HasPrecision(18, 2);
            financials.Property(f => f.Ebitda).HasPrecision(18, 2);
            financials.Property(f => f.UsdDebt).HasPrecision(18, 2);
        });

        builder.HasMany(o => o.Members).WithOne().HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Groups).WithOne().HasForeignKey(g => g.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Rules).WithOne().HasForeignKey(r => r.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Commodities).WithOne().HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}

