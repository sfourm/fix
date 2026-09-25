namespace Fix.Infrastructure.Persistence.Auditing;

/// <summary>
/// Shadow properties de auditoria adicionadas a todas as entidades do domínio
/// (colunas created_at, updated_at, author_created, author_updated).
/// </summary>
public static class AuditProperties
{
    public const string CreatedAt = "CreatedAt";
    public const string UpdatedAt = "UpdatedAt";
    public const string AuthorCreated = "AuthorCreated";
    public const string AuthorUpdated = "AuthorUpdated";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        CreatedAt,
        UpdatedAt,
        AuthorCreated,
        AuthorUpdated,
    };
}
