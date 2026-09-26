namespace Fix.Storage.Domain.Abstractions;

public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public PagedList<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToList(), Page, PageSize, TotalCount);
}
