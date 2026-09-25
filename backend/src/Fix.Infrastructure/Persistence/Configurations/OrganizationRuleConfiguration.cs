using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationRuleConfiguration : IEntityTypeConfiguration<OrganizationRule>
{
    public void Configure(EntityTypeBuilder<OrganizationRule> builder)
    {
        builder.ToTable("organization_rules", t => t.HasCheckConstraint(
            "ck_organization_rules_single_target",
            "(member_id IS NULL) <> (group_id IS NULL)"));
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.OrganizationId, r.RuleId, r.MemberId })
            .IsUnique()
            .HasFilter("member_id IS NOT NULL");
        builder.HasIndex(r => new { r.OrganizationId, r.RuleId, r.GroupId })
            .IsUnique()
            .HasFilter("group_id IS NOT NULL");

        builder.HasOne<Rule>().WithMany().HasForeignKey(r => r.RuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrganizationMember>().WithMany().HasForeignKey(r => r.MemberId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrganizationGroup>().WithMany().HasForeignKey(r => r.GroupId).OnDelete(DeleteBehavior.Cascade);
    }
}

