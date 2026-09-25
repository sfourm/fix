using Fix.Domain.AggregateRoots.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationGroupMemberConfiguration : IEntityTypeConfiguration<OrganizationGroupMember>
{
    public void Configure(EntityTypeBuilder<OrganizationGroupMember> builder)
    {
        builder.ToTable("organization_group_members");
        builder.HasKey(gm => gm.Id);

        builder.HasIndex(gm => new { gm.GroupId, gm.MemberId }).IsUnique();

        builder.HasOne<OrganizationMember>().WithMany().HasForeignKey(gm => gm.MemberId).OnDelete(DeleteBehavior.Cascade);
    }
}

