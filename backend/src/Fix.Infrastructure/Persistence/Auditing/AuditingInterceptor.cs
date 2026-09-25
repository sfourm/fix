using System.Text.Json;
using Fix.Application.Abstractions.Context;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fix.Infrastructure.Persistence.Auditing;

/// <summary>
/// Preenche os campos de auditoria e grava a timeline do domínio de toda entidade criada, alterada ou removida.
/// Entidades filhas registram na timeline do agregado dono (com o nome da entidade como prefixo dos campos),
/// para que o histórico de um agregado mostre também as mudanças nas suas partes.
/// </summary>
public sealed class AuditingInterceptor(IRequestContext requestContext, TimeProvider timeProvider) : SaveChangesInterceptor
{
    private const string OrganizationIdProperty = nameof(IOrganizationScoped.OrganizationId);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Audit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Audit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Audit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        // O SaveChanges só chama DetectChanges depois dos interceptors.
        context.ChangeTracker.DetectChanges();

        var now = timeProvider.GetUtcNow();
        var authorId = requestContext.UserId;

        var entries = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not Timeline and not Role)
            .ToList();

        foreach (var entry in entries)
        {
            var state = entry.State;

            if (state == EntityState.Added)
            {
                entry.Property(AuditProperties.CreatedAt).CurrentValue = now;
                entry.Property(AuditProperties.AuthorCreated).CurrentValue = authorId;
            }
            else if (state == EntityState.Modified)
            {
                entry.Property(AuditProperties.UpdatedAt).CurrentValue = now;
                entry.Property(AuditProperties.AuthorUpdated).CurrentValue = authorId;
            }

            var target = ResolveTarget(entry);
            var changes = CollectChanges(entry, target.FieldPrefix);
            if (state == EntityState.Modified && changes.Count == 0)
            {
                continue;
            }

            var action = state switch
            {
                EntityState.Added => "Created",
                EntityState.Deleted => "Deleted",
                _ => "Updated",
            };

            context.Add(target.Create(
                ResolveOrganizationId(entry),
                target.EntityId,
                action,
                JsonSerializer.Serialize(changes),
                authorId,
                now));
        }
    }

    /// <summary>Timeline de destino: a do próprio agregado ou a do agregado dono da entidade filha.</summary>
    private static TimelineTarget ResolveTarget(EntityEntry<Entity> entry)
    {
        var entity = entry.Entity;
        Guid Owner(string property) => (Guid)(entry.State == EntityState.Deleted
            ? entry.Property(property).OriginalValue!
            : entry.Property(property).CurrentValue!);

        return entity switch
        {
            Organization => new(entity.Id, null, (o, e, a, s, u, t) => new OrganizationTimeline(o, e, a, s, u, t)),
            OrganizationMember or OrganizationRule or OrganizationCommodity =>
                new(Owner(OrganizationIdProperty), entry.Metadata.ClrType.Name, (o, e, a, s, u, t) => new OrganizationTimeline(o, e, a, s, u, t)),
            OrganizationGroup => new(entity.Id, null, (o, e, a, s, u, t) => new OrganizationGroupTimeline(o, e, a, s, u, t)),
            OrganizationGroupMember =>
                new(Owner(nameof(OrganizationGroupMember.GroupId)), nameof(OrganizationGroupMember), (o, e, a, s, u, t) => new OrganizationGroupTimeline(o, e, a, s, u, t)),
            Counterparty => new(entity.Id, null, (o, e, a, s, u, t) => new CounterpartyTimeline(o, e, a, s, u, t)),
            Policy => new(entity.Id, null, (o, e, a, s, u, t) => new PolicyTimeline(o, e, a, s, u, t)),
            PolicyAxis or CoverageBand or PolicyInstrument or PolicyVersion =>
                new(Owner(nameof(PolicyAxis.PolicyId)), entry.Metadata.ClrType.Name, (o, e, a, s, u, t) => new PolicyTimeline(o, e, a, s, u, t)),
            Mandate => new(entity.Id, null, (o, e, a, s, u, t) => new MandateTimeline(o, e, a, s, u, t)),
            Order => new(entity.Id, null, (o, e, a, s, u, t) => new OrderTimeline(o, e, a, s, u, t)),
            Rule => new(entity.Id, null, (o, e, a, s, u, t) => new RuleTimeline(o, e, a, s, u, t)),
            RuleRole => new(Owner(nameof(RuleRole.RuleId)), nameof(RuleRole), (o, e, a, s, u, t) => new RuleTimeline(o, e, a, s, u, t)),
            _ => throw new InvalidOperationException($"Não existe timeline mapeada para a entidade '{entry.Metadata.ClrType.Name}'."),
        };
    }

    /// <summary>
    /// Organização da alteração: a própria organização, o OrganizationId da entidade ou, para filhos
    /// sem essa coluna (ex.: eixo da política), a organização do agregado dono já rastreado.
    /// </summary>
    private static Guid? ResolveOrganizationId(EntityEntry<Entity> entry)
    {
        if (entry.Entity is Organization organization)
        {
            return organization.Id;
        }

        if (entry.Metadata.FindProperty(OrganizationIdProperty) is not null)
        {
            var property = entry.Property(OrganizationIdProperty);
            return (Guid?)(entry.State == EntityState.Deleted ? property.OriginalValue : property.CurrentValue);
        }

        var tracker = entry.Context.ChangeTracker;
        Guid? PolicyOrganization(Guid policyId) =>
            tracker.Entries<Policy>().FirstOrDefault(p => p.Entity.Id == policyId)?.Entity.OrganizationId;

        return entry.Entity switch
        {
            PolicyAxis axis => PolicyOrganization(axis.PolicyId),
            CoverageBand band => PolicyOrganization(band.PolicyId),
            PolicyInstrument instrument => PolicyOrganization(instrument.PolicyId),
            PolicyVersion version => PolicyOrganization(version.PolicyId),
            OrganizationGroupMember member =>
                tracker.Entries<OrganizationGroup>().FirstOrDefault(g => g.Entity.Id == member.GroupId)?.Entity.OrganizationId,
            _ => null,
        };
    }

    private static Dictionary<string, object?> CollectChanges(EntityEntry entry, string? prefix)
    {
        var changes = new Dictionary<string, object?>();
        var keyPrefix = prefix is null ? string.Empty : $"{prefix}.";

        foreach (var property in entry.Properties)
        {
            Collect(property, keyPrefix + property.Metadata.Name);
        }

        foreach (var complex in entry.ComplexProperties)
        {
            foreach (var property in complex.Properties)
            {
                Collect(property, $"{keyPrefix}{complex.Metadata.Name}.{property.Metadata.Name}");
            }
        }

        return changes;

        void Collect(PropertyEntry property, string name)
        {
            if (AuditProperties.All.Contains(property.Metadata.Name) || property.Metadata.IsPrimaryKey())
            {
                return;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    changes[name] = ToProvider(property, property.CurrentValue);
                    break;
                case EntityState.Deleted:
                    changes[name] = ToProvider(property, property.OriginalValue);
                    break;
                case EntityState.Modified when property.IsModified:
                    changes[name] = new
                    {
                        old = ToProvider(property, property.OriginalValue),
                        @new = ToProvider(property, property.CurrentValue),
                    };
                    break;
            }
        }
    }

    // Value objects e enums são convertidos para o valor que vai ao banco (ex.: Title -> string).
    private static object? ToProvider(PropertyEntry property, object? value) =>
        value is null ? null : property.Metadata.GetTypeMapping().Converter?.ConvertToProvider(value) ?? value;

    private delegate Timeline TimelineFactory(
        Guid? organizationId,
        Guid entityId,
        string action,
        string snapshot,
        Guid? authorId,
        DateTimeOffset occurredAt);

    private sealed record TimelineTarget(Guid EntityId, string? FieldPrefix, TimelineFactory Create);
}
