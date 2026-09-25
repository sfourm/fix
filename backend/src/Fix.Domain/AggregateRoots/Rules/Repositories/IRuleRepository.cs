using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Domain.AggregateRoots.Rules.Repositories;

public interface IRuleRepository
{
    Task<IReadOnlyList<Rule>> ListAsync(CancellationToken cancellationToken);

    Task<Rule?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
