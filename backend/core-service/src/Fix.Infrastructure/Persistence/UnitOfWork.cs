using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fix.Infrastructure.Persistence;

internal sealed class UnitOfWork(FixDbContext dbContext, IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());

        int affected;
        try
        {
            affected = await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new ConflictException($"Registro duplicado ({pg.ConstraintName}).");
        }

        // Eventos são despachados após o commit (efeitos colaterais só acontecem se a transação persistiu).
        await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return affected;
    }
}
