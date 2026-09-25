using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.AggregateRoots.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fix.Infrastructure.Persistence.Configurations;

/// <summary>
/// Timelines do domínio numa única tabela (TPH): o tipo da entidade auditada é o discriminador.
/// </summary>
internal sealed class TimelineConfiguration : IEntityTypeConfiguration<Timeline>
{
    public void Configure(EntityTypeBuilder<Timeline> builder)
    {
        builder.ToTable("timelines");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(t => t.Action).HasMaxLength(16).IsRequired();
        builder.Property(t => t.Snapshot).HasColumnType("jsonb").IsRequired();

        builder.HasDiscriminator(t => t.EntityType)
            .HasValue<OrganizationTimeline>(nameof(Organization))
            .HasValue<OrganizationGroupTimeline>(nameof(OrganizationGroup))
            .HasValue<CounterpartyTimeline>(nameof(Counterparty))
            .HasValue<PolicyTimeline>(nameof(Policy))
            .HasValue<MandateTimeline>(nameof(Mandate))
            .HasValue<OrderTimeline>(nameof(Order))
            .HasValue<RuleTimeline>(nameof(Rule));

        builder.HasIndex(t => new { t.OrganizationId, t.OccurredAt });
        builder.HasIndex(t => new { t.EntityType, t.EntityId });
    }
}
