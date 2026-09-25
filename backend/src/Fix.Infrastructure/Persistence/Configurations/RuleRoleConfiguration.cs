using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

internal sealed class RuleRoleConfiguration : IEntityTypeConfiguration<RuleRole>
{
    public void Configure(EntityTypeBuilder<RuleRole> builder)
    {
        builder.ToTable("rule_roles");
        builder.HasKey(rr => rr.Id);

        builder.HasIndex(rr => new { rr.RuleId, rr.RoleId }).IsUnique();
        builder.HasOne<Role>().WithMany().HasForeignKey(rr => rr.RoleId).OnDelete(DeleteBehavior.Cascade);

        builder.HasData(SystemRules.All.SelectMany(rule => rule.Roles.Select(role => new
        {
            Id = SystemRules.RuleRoleId(rule.Code, role),
            RuleId = SystemRules.Id(rule.Code),
            RoleId = SystemRoles.Id(role),
            SeedData.CreatedAt,
        })));
    }
}

