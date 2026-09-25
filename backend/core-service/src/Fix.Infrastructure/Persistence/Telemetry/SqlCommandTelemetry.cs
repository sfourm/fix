using System.Diagnostics;
using System.Text.RegularExpressions;
using Npgsql;

namespace Fix.Infrastructure.Persistence.Telemetry;

/// <summary>
/// Dá nome útil aos spans do Npgsql ("SELECT orders" em vez de "postgresql") e grava operação e tabela como atributos,
/// para os traces e as métricas por operação (spanmetrics) mostrarem o que cada request fez no banco.
/// </summary>
internal static partial class SqlCommandTelemetry
{
    private const string Fallback = "postgresql";

    public static void Configure(NpgsqlTracingOptionsBuilder tracing) => tracing
        .ConfigureCommandSpanNameProvider(command => SpanName(Describe(command.CommandText)))
        .ConfigureCommandEnrichmentCallback((activity, command) => Enrich(activity, Describe(command.CommandText)))
        .ConfigureBatchSpanNameProvider(batch => BatchName(batch))
        .ConfigureBatchEnrichmentCallback((activity, batch) =>
        {
            Enrich(activity, Describe(batch.BatchCommands.Count > 0 ? batch.BatchCommands[0].CommandText : null));
            activity.SetTag("db.operation.batch.size", batch.BatchCommands.Count);
        });

    private static string BatchName(NpgsqlBatch batch)
    {
        var first = SpanName(Describe(batch.BatchCommands.Count > 0 ? batch.BatchCommands[0].CommandText : null));
        return batch.BatchCommands.Count > 1 ? $"BATCH {first} (+{batch.BatchCommands.Count - 1})" : first;
    }

    private static string SpanName((string? Operation, string? Table) sql) => sql switch
    {
        ({ } operation, { } table) => $"{operation} {table}",
        ({ } operation, null) => operation,
        _ => Fallback,
    };

    private static void Enrich(Activity activity, (string? Operation, string? Table) sql)
    {
        if (sql.Operation is not null) activity.SetTag("db.operation.name", sql.Operation);
        if (sql.Table is not null) activity.SetTag("db.collection.name", sql.Table);
    }

    private static (string? Operation, string? Table) Describe(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return (null, null);

        var operation = Operation().Match(sql);
        if (!operation.Success) return (null, null);

        var name = operation.Groups["op"].Value.ToUpperInvariant();
        var table = name switch
        {
            "SELECT" or "DELETE" => From().Match(sql),
            "INSERT" => Into().Match(sql),
            "UPDATE" => Update().Match(sql),
            _ => Match.Empty,
        };

        return (name, table.Success ? table.Groups["table"].Value : null);
    }

    [GeneratedRegex(@"^\s*(?<op>SELECT|INSERT|UPDATE|DELETE|BEGIN|COMMIT|ROLLBACK|SAVEPOINT|RELEASE|CREATE|ALTER|DROP|WITH)\b", RegexOptions.IgnoreCase)]
    private static partial Regex Operation();

    [GeneratedRegex(@"\bFROM\s+(?:""?\w+""?\.)?""?(?<table>\w+)""?", RegexOptions.IgnoreCase)]
    private static partial Regex From();

    [GeneratedRegex(@"\bINTO\s+(?:""?\w+""?\.)?""?(?<table>\w+)""?", RegexOptions.IgnoreCase)]
    private static partial Regex Into();

    [GeneratedRegex(@"^\s*UPDATE\s+(?:""?\w+""?\.)?""?(?<table>\w+)""?", RegexOptions.IgnoreCase)]
    private static partial Regex Update();
}
