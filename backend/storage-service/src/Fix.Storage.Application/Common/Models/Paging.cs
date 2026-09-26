namespace Fix.Storage.Application.Common;

public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 200;

    public static (int Page, int PageSize) Normalize(int page, int pageSize) =>
        (Math.Max(1, page), pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize));
}
