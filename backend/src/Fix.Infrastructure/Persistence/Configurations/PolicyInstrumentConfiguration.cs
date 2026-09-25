using Fix.Domain.AggregateRoots.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class PolicyInstrumentConfiguration : IEntityTypeConfiguration<PolicyInstrument>
{
    public void Configure(EntityTypeBuilder<PolicyInstrument> builder)
    {
        builder.ToTable("policy_instruments");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name).HasMaxLength(150).IsRequired();
        builder.Property(i => i.Permission).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Condition).HasMaxLength(300);
    }
}

