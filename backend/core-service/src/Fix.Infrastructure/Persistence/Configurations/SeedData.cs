namespace Fix.Infrastructure.Persistence.Configurations;

/// <summary>Os objetos anônimos do seed usam o mesmo nome da shadow property de auditoria (CreatedAt).</summary>
internal static class SeedData
{
    public static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
