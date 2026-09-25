namespace Fix.Application.Common;

internal static class Clock
{
    /// <summary>Data de hoje (UTC) para regras de calendário: vigência, horizonte, prazos.</summary>
    public static DateOnly Today(this TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
