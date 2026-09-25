using Fix.Application.Abstractions.Context;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.AggregateRoots.Rules;
using Fix.Infrastructure.Identity;
using Fix.Infrastructure.Persistence.Auditing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence;

public sealed class FixDbContext(DbContextOptions<FixDbContext> options, IRequestContext requestContext)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    public DbSet<OrganizationGroup> OrganizationGroups => Set<OrganizationGroup>();

    public DbSet<OrganizationGroupMember> OrganizationGroupMembers => Set<OrganizationGroupMember>();

    public DbSet<OrganizationRule> OrganizationRules => Set<OrganizationRule>();

    public DbSet<OrganizationCommodity> OrganizationCommodities => Set<OrganizationCommodity>();

    // As roles (permissões atômicas) são acessadas por Set<Role>(): o nome "Roles" já é das roles do Identity.
    public DbSet<Rule> Rules => Set<Rule>();

    public DbSet<RuleRole> RuleRoles => Set<RuleRole>();

    public DbSet<Counterparty> Counterparties => Set<Counterparty>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<Mandate> Mandates => Set<Mandate>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Timeline> Timelines => Set<Timeline>();

    public DbSet<OrganizationTimeline> OrganizationTimelines => Set<OrganizationTimeline>();

    public DbSet<OrganizationGroupTimeline> OrganizationGroupTimelines => Set<OrganizationGroupTimeline>();

    public DbSet<CounterpartyTimeline> CounterpartyTimelines => Set<CounterpartyTimeline>();

    public DbSet<PolicyTimeline> PolicyTimelines => Set<PolicyTimeline>();

    public DbSet<MandateTimeline> MandateTimelines => Set<MandateTimeline>();

    public DbSet<OrderTimeline> OrderTimelines => Set<OrderTimeline>();

    public DbSet<RuleTimeline> RuleTimelines => Set<RuleTimeline>();

    /// <summary>Usado pelos query filters: sem tenant na requisição, nenhum dado de tenant é retornado.</summary>
    private Guid? CurrentOrganizationId => requestContext.OrganizationId;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(FixDbContext).Assembly);

        ConfigureIdentityTables(builder);

        builder.Entity<Counterparty>().HasQueryFilter(c => c.OrganizationId == CurrentOrganizationId);
        builder.Entity<Policy>().HasQueryFilter(p => p.OrganizationId == CurrentOrganizationId);
        builder.Entity<Mandate>().HasQueryFilter(m => m.OrganizationId == CurrentOrganizationId);
        builder.Entity<Order>().HasQueryFilter(o => o.OrganizationId == CurrentOrganizationId);

        ApplyDomainConventions(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("users");
            b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
        });
        builder.Entity<IdentityRole<Guid>>().ToTable("identity_roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("identity_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("identity_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("identity_user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("identity_user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("identity_role_claims");
    }

    /// <summary>
    /// Convenções para as entidades do domínio: ids gerados pelo domínio, eventos ignorados
    /// e as colunas de auditoria como shadow properties. As timelines já são o registro de auditoria
    /// (autor e data próprios) e os tipos derivados de TPH herdam a configuração da raiz.
    /// </summary>
    private static void ApplyDomainConventions(ModelBuilder builder)
    {
        var domainEntityTypes = builder.Model.GetEntityTypes()
            .Where(t => typeof(Entity).IsAssignableFrom(t.ClrType)
                && !typeof(Timeline).IsAssignableFrom(t.ClrType)
                && t.BaseType is null)
            .Select(t => t.ClrType)
            .ToList();

        builder.Entity<Timeline>().Property(t => t.Id).ValueGeneratedNever();

        foreach (var clrType in domainEntityTypes)
        {
            var entity = builder.Entity(clrType);

            entity.Property(nameof(Entity.Id)).ValueGeneratedNever();

            if (typeof(AggregateRoot).IsAssignableFrom(clrType))
            {
                entity.Ignore(nameof(AggregateRoot.DomainEvents));
            }

            entity.Property<DateTimeOffset>(AuditProperties.CreatedAt).IsRequired();
            entity.Property<DateTimeOffset?>(AuditProperties.UpdatedAt);
            entity.Property<Guid?>(AuditProperties.AuthorCreated);
            entity.Property<Guid?>(AuditProperties.AuthorUpdated);
        }
    }
}

