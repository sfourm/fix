using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationGroupConfiguration : IEntityTypeConfiguration<OrganizationGroup>
{
    public void Configure(EntityTypeBuilder<OrganizationGroup> builder)
    {
        builder.ToTable("organization_groups");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasConversion(ValueObjectConversions.Name)
            .HasMaxLength(Name.MaxLength)
            .IsRequired();
        builder.HasIndex(g => new { g.OrganizationId, g.Name }).IsUnique();

        builder.HasMany(g => g.Members).WithOne().HasForeignKey(gm => gm.GroupId).OnDelete(DeleteBehavior.Cascade);
    }
}

